��# Compiler

Navigate to https://github.com/Metallicruz/Compiler/tree/master/Compiler+VM where you find the excecutable for the compiler as well as the virtual machine and assembler I wrote. The files accepted by the compiler use the extention .pxi and I have include an sample file called "factorial.pxi"

To run a pxi program first compile it by running the compiler passing the kxi file as a parameter: ./Compiler [name of file].pxi
If the syntax and semantics in the file are correct the compiler will create an intermediate code (ICODE) file and then create an assembly (asm) file.
The final step is to run the assembly file with the virtual machine excecutable (vm.exe) which also contains an assembler. This can be done by typing: ./vm [name of file].asm
