// Project Prolog
// Name: Roberto De La Cruz
// CS4380
// Project: proj3
// 
// I declare that the following code was written by me or provided 
// by the instructor for this project. I understand that copying source
// code from any other source constitutes cheating, and that I will receive
// a zero on this project if I am found in violation of this policy.
// ---------------------------------------------------------------------------


#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <cstring>
#include <map>
#include <vector>
#include <array>

namespace Assembler {
	char* mem;
	int codeStartPC;
	int codeEndPC;
	int mainStartPC;
	int* test;
	int* testChar;
	std::map<std::string, int> table;
	const int NUM_OP_CODES = 25;
	const int NUM_REGISTERS = 13;
	const int CODE_SIZE = 10;
	const int INDIRECT_OFFSET = 13;
	const char delimiter = ' ';
	enum OpCode { JMP, JMR, BNZ, BGT, BLT, BRZ, MOV, LDA, STR, LDR, STB, LDB, ADD, ADI, SUB, MUL, DIV, AND, OR, CMP, TRP, STRI, LDRI, STBI, LDBI };
	enum Register { R0, R1, R2, R3, R4, R5, R6, R7, PC, SL, SP, FP, SB };
	std::string registers[NUM_REGISTERS] = { "R0","R1","R2","R3","R4","R5","R6","R7", "PC", "SL", "SP", "FP", "SB" };
	std::string OpCodes[NUM_OP_CODES] = { "JMP","JMR","BNZ","BGT","BLT","BRZ","MOV","LDA","STR","LDR","STB","LDB","ADD","ADI","SUB","MUL","DIV","AND","OR","CMP","TRP" };

	void Assembler(char* memory) {
		mem = memory;
	}

	//Purpose: to check if string is a label
	//Parameters: a string to check
	//Returns: 0 if not a label or 1 if it is a label
	int IsLabel(std::string s) {
		if (s == ".INT" || s == ".BYT") {
			return 0;
		}
		for (int i = 0; i < NUM_OP_CODES; ++i) {
			if (s == OpCodes[i]) {
				return 0;
			}
		}
		return 1;
	}

	//Purpose: to take a string from assembly file and tokenize it
	//Parameters: assembly file line as string
	std::vector<std::string> GetTokens(std::string line) {
		std::vector<std::string> tokens;
		std::string token;
		std::stringstream ss;
		ss.str(line);
		while (std::getline(ss, token, delimiter)) {
			tokens.push_back(token);
		}
		return tokens;
	}

	//Purpose: add label to symbol table
	//Parameters: label and program counter
	void TableAdd(std::string token, int pc) {
		//std::cout << token << " " << pc << std::endl;
		if (table.find(token) == table.end()) {
			table.insert(std::pair<std::string, int>(token, pc));
		}
	}

	//Purpose: first pass on assembly file, creates symbol table
	//Parameters: assembly file name as a string
	void Pass1(std::string file) {
		std::vector<std::string> tokens;
		std::string input;
		std::ifstream ifs;
		std::string fileName;
		int pc = 0;
		mainStartPC = 0;
		codeEndPC = 0;
		fileName = file;
		ifs.open(fileName);
		if (ifs.good()) {
			while (std::getline(ifs, input)) {
				if (input != "")
				{
					tokens = GetTokens(input);
					int valueIndex = 0;
					if (IsLabel(tokens[0])) {//current assembly line contains a label	
						++valueIndex;
						TableAdd(tokens[0], pc);
					}
					if (tokens[valueIndex] == ".INT") {
						pc += sizeof(int);
					}
					else if (tokens[valueIndex] == ".BYT") {
						pc += sizeof(char);
					}
					else {
						for (int i = 0; i < NUM_OP_CODES; ++i) {
							if (OpCodes[i] == tokens[valueIndex]) {
								pc += 3 * sizeof(int);
								break;
							}
						}
						if (mainStartPC == 0) {//default start of code segment
							mainStartPC = pc - 3 * sizeof(int);
						}
					}
				}
			}
			if (codeEndPC == 0) {//default end of code segment
				codeEndPC = pc - 3 * sizeof(int);
			}
		}
		ifs.close();
	}

	int GetRegister(std::string reg) {
		for (int i = 0; i < NUM_REGISTERS; ++i) {
			if (reg == registers[i]) {
				return i;
			}
		}
	}

	void RegisterRegister(int opCode, int opCodeIndex, int& pc, std::vector<std::string>& line) {
		int* insert = (int*)(mem + pc);
		*insert = opCode;
		pc += sizeof(int);
		insert = (int*)(mem + pc);

		//test = (int*)(mem + pc - 4);
		//std::cout << *test << std::endl;

		*insert = GetRegister(line[opCodeIndex + 1]);
		pc += sizeof(int);
		insert = (int*)(mem + pc);

		//test = (int*)(mem + pc - 4);
		//std::cout << *test << std::endl;

		*insert = GetRegister(line[opCodeIndex + 2]);
		pc += sizeof(int);

		//test = (int*)(mem + pc - 4);
		//std::cout << *test << std::endl;
	}

	void WriteMemory(std::vector<std::string>& line, int& pc) {
		int opCodeIndex = 0;
		if (IsLabel(line[0])) {//current assembly line contains a label
			opCodeIndex = 1;
		}
		if (line[0] == "CODE_START") {
			return;
		}
		else if (line[opCodeIndex] == ".INT") {
			int* insert = (int*)(mem + pc);
			*insert = atoi(line[opCodeIndex + 1].c_str());
			pc += sizeof(int);


			//int* value2 = (int*)(mem + pc - 4);
			//int value1 = *value2;
			//std::cout << *value2 << std::endl;
		}
		else if (line[opCodeIndex] == ".BYT") {
			const char* value = line[opCodeIndex + 1].c_str();
			if (value[0] == '\\' && value[1] == 'n') {
				mem[pc++] = '\n';
			}
			else if (value[0] == '\0') {
				mem[pc++] = ' ';
			}
			else {
				mem[pc++] = value[0];
			}
			//std::cout << mem[pc-1] << std::endl;
		}
		else {
			if (codeStartPC == 0) {
				codeStartPC = pc;
				//std::cout << "~Start of Code: " << codeStartPC << std::endl;
			}
			for (int i = 0; i < NUM_OP_CODES; ++i) {
				if (line[opCodeIndex] == OpCodes[i]) {
					int* insert = (int*)(mem + pc);
					int temp;
					const char* value;
					bool indirect = false;
					//int* test;
					switch (i) {
					case STR:
					case LDR:
					case STB:
					case LDB:
						for each (std::string s in registers)
						{
							if (s == line[opCodeIndex + 2]) {
								indirect = true;
								break;
							}
						}
						/*value = line[opCodeIndex + 2].c_str();
						temp = value[1];
						if (value[0] == 'R' && temp >= (char)'0' && temp <= (char)'9') {
						indirect = true;
						}*/
					case LDA:
					case BNZ:
					case BGT:
					case BLT:
					case BRZ:
						*insert = i + 1; //op code starts at 1 but enum starts at 0 to line up with arrays
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;

						*insert = GetRegister(line[opCodeIndex + 1]);
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;
						if (indirect) {
							pc = pc - (sizeof(int) + sizeof(int));
							RegisterRegister(i + INDIRECT_OFFSET + 1, opCodeIndex, pc, line);
						}
						else {
							if (line[opCodeIndex + 2] == "PC") {
								*insert = pc;
							}
							else {
								if (table.find(line[opCodeIndex + 2]) == table.end()) {
									std::cout << "Error Loading Label: " << line[opCodeIndex + 2] << " doesn't exist";
									exit(1);
								}
								*insert = table.at(line[opCodeIndex + 2]);
							}
							pc += sizeof(int);
						}

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;
						break;
					case TRP:
						*insert = TRP + 1;
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						*insert = atoi(line[opCodeIndex + 1].c_str());
						pc += sizeof(int);
						pc += sizeof(int);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;
						break;
					case JMP:
						*insert = JMP + 1;
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						if (table.find(line[opCodeIndex + 1]) == table.end()) {
							std::cout << "Error Finding Label: " << line[opCodeIndex + 1] << " doesn't exist";
							exit(1);
						}
						*insert = table.at(line[opCodeIndex + 1]);
						pc += sizeof(int);
						pc += sizeof(int);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;
						break;
					case JMR:
						*insert = JMR + 1;
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;

						*insert = GetRegister(line[opCodeIndex + 1]);
						pc += sizeof(int);
						pc += sizeof(int);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;
						break;
					case ADI:
						*insert = ADI + 1; //op code starts at 1 but enum starts at 0 to line up with arrays
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;

						*insert = GetRegister(line[opCodeIndex + 1]);
						pc += sizeof(int);
						insert = (int*)(mem + pc);

						//test = (int*)(mem + pc - 4);
						//std::cout << *test << std::endl;

						*insert = atoi(line[opCodeIndex + 2].c_str());
						pc += sizeof(int);
						break;
					default://arithmetic operations
						RegisterRegister(i + 1, opCodeIndex, pc, line); //add 1 since op code starts at 1 but enum starts at 0
						break;
					}
				}
			}
		}
		/*int stuff = 300;
		char* charstuff = new char[8];
		int* value = (int*)charstuff;
		*value = stuff;
		int intstuff = *value;*/

	}

	//Purpose: second pass on assembly file, create byte code
	//Parameters: assembly file name as a string
	//returns: starting point for data
	int Pass2(std::string file) {
		std::string input;
		std::ifstream ifs;
		std::string fileName;
		int pc = 0;
		codeStartPC = 0;
		fileName = file;
		ifs.open(fileName);
		if (ifs.good()) {
			std::vector<std::string> tokens;
			while (std::getline(ifs, input)) {
				if (input != "") {
					tokens = GetTokens(input);
					WriteMemory(tokens, pc);
				}
			}
		}
		ifs.close();
		//int value = mem[codeStartPC];
		//std::cout << value;
		return mainStartPC;
	}

};





namespace VM {
	int IR = 0;
	int opd1 = 0;
	int opd2 = 0;
	int opCode = 0;
	int* test = 0;
	int* intValue;
	char* byteValue;
	char* buffer = new char[1000];
	bool running = false;
	const int NUM_OF_REG = 13;
	const int MEM_SIZE = 10000000;
	const int STACK_SIZE = 100000;
	char* mem = new char[MEM_SIZE];
	int* reg = new int[NUM_OF_REG];

	int Fetch(int& pc) {
		int* inst;
		inst = (int*)(mem + pc);
		pc += sizeof(int);
		//std::cout <<*inst;
		return *inst;
	}

	char FetchByte(int& pc) {
		char* inst;
		inst = (char*)(mem + pc);
		pc += sizeof(char);
		//std::cout <<*inst;
		return *inst;
	}

	void Start(std::string fileName) {
		Assembler::Assembler(mem);
		Assembler::Pass1(fileName);
		int codeStart = Assembler::Pass2(fileName);
		reg[Assembler::PC] = codeStart;
		reg[Assembler::SL] = MEM_SIZE - 1 - STACK_SIZE;
		reg[Assembler::SP] = MEM_SIZE - 1;
		reg[Assembler::FP] = MEM_SIZE - 1;
		reg[Assembler::SB] = MEM_SIZE - 1;
		running = true;
		while (running) {
			IR = Fetch(reg[Assembler::PC]);
			opd1 = Fetch(reg[Assembler::PC]);
			opd2 = Fetch(reg[Assembler::PC]);
			switch (IR) {
			case Assembler::ADD + 1://op code starts at 1 but enum starts at 0
									//std::cout << reg[opd1];
				reg[opd1] = reg[opd1] + reg[opd2];
				//std::cout << reg[opd1] <<'+'<<reg[opd2]<<std::endl;
				break;
			case Assembler::ADI + 1://op code starts at 1 but enum starts at 0
									//std::cout << reg[opd1];
				reg[opd1] = reg[opd1] + opd2;
				//std::cout << reg[opd1] <<'+'<<reg[opd2]<<std::endl;
				break;
			case Assembler::SUB + 1://op code starts at 1 but enum starts at 0
									//std::cout << reg[opd1];
				reg[opd1] = reg[opd1] - reg[opd2];
				//std::cout << " - " << reg[opd2] << std::endl;
				break;
			case Assembler::MUL + 1://op code starts at 1 but enum starts at 0
				reg[opd1] = reg[opd1] * reg[opd2];
				break;
			case Assembler::DIV + 1://op code starts at 1 but enum starts at 0
				reg[opd1] = reg[opd1] / reg[opd2];
				break;
			case Assembler::MOV + 1://op code starts at 1 but enum starts at 0
				reg[opd1] = reg[opd2];
				break;
			case Assembler::STR + 1://op code starts at 1 but enum starts at 0
				intValue = (int*)(mem + opd2);
				*intValue = reg[opd1];
				//std::cout<<test<<std::endl;
				break;
			case Assembler::LDR + 1://op code starts at 1 but enum starts at 0
									//std::cout << "LDR " << opd2 << std::endl;
				intValue = (int*)(mem + opd2);
				reg[opd1] = *intValue;
				break;
			case Assembler::STB + 1://op code starts at 1 but enum starts at 0
				byteValue = (char*)(mem + opd2);
				*byteValue = reg[opd1];
				//std::cout<<test<<std::endl;
				break;
			case Assembler::LDB + 1://op code starts at 1 but enum starts at 0
				byteValue = (char*)(mem + opd2);
				reg[opd1] = (int)*byteValue;
				//std::cout<<test<<std::endl;
				break;
			case Assembler::STRI + 1://op code starts at 1 but enum starts at 0
				intValue = (int*)(mem + reg[opd2]);
				*intValue = reg[opd1];
				//std::cout<<test<<std::endl;
				break;
			case Assembler::LDRI + 1://op code starts at 1 but enum starts at 0
				intValue = (int*)(mem + reg[opd2]);
				reg[opd1] = *intValue;
				//std::cout<<"LDRI"<<reg[opd1]<<std::endl;
				break;
			case Assembler::STBI + 1://op code starts at 1 but enum starts at 0
									 //std::cout<< reg[opd1] << reg[opd2] <<std::endl;
				byteValue = (char*)(mem + reg[opd2]);
				*byteValue = reg[opd1];
				//std::cout << *byteValue << std::endl;
				//std::cout<<test<<std::endl;
				break;
			case Assembler::LDBI + 1://op code starts at 1 but enum starts at 0
				byteValue = (char*)(mem + reg[opd2]);
				reg[opd1] = (int)*byteValue;
				//std::cout<<test<<std::endl;
				break;
			case Assembler::LDA + 1://op code starts at 1 but enum starts at 0
									//std::cout << "R" << 7 << reg[7] << std::endl;
				reg[opd1] = opd2;
				//std::cout<<"R"<<opd1<<" LDA "<<reg[opd1]<<std::endl;
				break;
			case Assembler::CMP + 1://op code starts at 1 but enum starts at 0
				reg[opd1] -= reg[opd2];
				break;
			case Assembler::JMP + 1://op code starts at 1 but enum starts at 0
				reg[Assembler::PC] = opd1;
				break;
			case Assembler::JMR + 1://op code starts at 1 but enum starts at 0
				reg[Assembler::PC] = reg[opd1];
				break;
			case Assembler::BGT + 1://op code starts at 1 but enum starts at 0
				if (reg[opd1] > 0) {
					reg[Assembler::PC] = opd2;
				}
				break;
			case Assembler::BLT + 1://op code starts at 1 but enum starts at 0
				if (reg[opd1] < 0) {
					reg[Assembler::PC] = opd2;
				}
				break;
			case Assembler::BRZ + 1://op code starts at 1 but enum starts at 0
				if (reg[opd1] == 0) {
					reg[Assembler::PC] = opd2;
					test = (int*)(mem + reg[Assembler::PC]);
					//std::cout<<"BRZ to "<<reg[Assembler::PC]<<std::endl;
				}
				break;
			case Assembler::BNZ + 1://op code starts at 1 but enum starts at 0
				if (reg[opd1] != 0) {
					reg[Assembler::PC] = opd2;
				}
				break;
			case Assembler::TRP + 1://op code starts at 1 but enum starts at 0
				char value;
				std::string input;
				std::string num;
				switch (opd1) {
				case 1:
					std::cout << reg[3];
					break;
				case 2:
					std::getline(std::cin, input);
					try {
						reg[3] = std::stoi(input);
					}
					catch (...) {
						reg[3] = -1;
					}
					break;
				case 3:
					value = reg[3];
					std::cout << value;
					break;
				case 4:
					std::getline(std::cin, input);
					reg[3] = input[0];
					//reg[3] = getchar();
					//std::cin >> reg[3];
					break;
				case 10:
					if (reg[3] < 48 || reg[3] > 57) {
						reg[3] = -1;
					}
					else {
						reg[3] -= 48;
					}
					break;
				case 11:
					reg[3] += 48;
					break;
				}
				break;
			}
			if (reg[Assembler::PC] > Assembler::codeEndPC) {
				running = false;
			}
		}
	}
}


#include <fstream>
#include <iostream>
#include <string>
#include <sstream>

int main(int argc, char* argv[]) {

	std::string fileName;
	std::ifstream ifs;
	if (argc > 1) {
		fileName = argv[1];
	}
	else {
		fileName = "test.asm";
	}
	ifs.open(fileName);
	if (!ifs.good()) {
		std::cout << "Invalid File " << fileName;
		return 1;
	}

	VM::Start(fileName);
	return 0;
}