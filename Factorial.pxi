class Math {
    Math() {}
	public int factorial(int n){
		if(n<=1){
			return 1;
		}
		else{
			return (n * factorial(n-1));
		}
	}
}
class Message {
    private char msg[];
    private int i;
    private int end;

    Message() {
    	msg = new char[100];
		msg[0] = 'E';
		msg[1] = 'n';
		msg[2] = 't';
		msg[3] = 'e';
		msg[4] = 'r';
		msg[5] = ' ';
		msg[6] = '!';
		msg[7] = ' ';
		msg[8] = 'T';
		msg[9] = 'o';
		msg[10] = ' ';
		msg[11] = 'Q';
		msg[12] = 'u';
		msg[13] = 'i';
		msg[14] = 't';
		msg[15] = 'F';
		msg[16] = 'a';
		msg[17] = 'c';
		msg[18] = 't';
		msg[19] = ':';
		msg[20] = ' ';
		msg[21] = '=';
		msg[22] = ' ';
		msg[23] = 'I';
		msg[24] = 'n';
		msg[25] = 'v';
		msg[26] = 'a';
		msg[27] = 'l';
		msg[28] = 'i';
		msg[29] = 'd';
		msg[30] = ' ';
		msg[31] = 'E';
		msg[32] = 'n';
		msg[33] = 't';
		msg[34] = 'r';
		msg[35] = 'y';
	}
    public void quit() {
		print(0, 14);
    }
    public void prompt() {
		cout << '\n';
		print(15, 20);
    }
    public void invalid() {
		cout << '\n';
		print(23, 35);
    }
    private void print(int i, int last) {
		while (i <= last) {
			cout << msg[i];
			i = i + 1;
		}
	}
}
void pxi main() {
	char key;
	int value;
    Message msg = new Message();
    Math math = new Math();
	msg.quit();
	msg.prompt();
	cin >> key;
    while (key != '!') {
		value = atoi(key);//single digit
		//cin >> value;//multi digit
		//if(value == -1){
			//msg.invalid();
			//return;
		//}
		cout << math.factorial(value);
		msg.prompt();
		cin >> key;
	}
}
