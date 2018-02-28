Btrue .INT 1
N1 .INT 1
N0 .INT 0
N4 .INT 4
N-1 .INT -1
H! .BYT !
FREE .INT 0
;------------------------END OF DATA SEGMENT-----------------------
LDR R1 N1 ;====25
LDR R0 N0 ;====37
LDA R2 END ;====49
ADI R2 36 ;====61
STR R2 FREE ;====73
MOV R2 FP ;====85
ADI R2 -4 ;====97
STR FP R2 ;====109
MOV R2 SP ;====121  ;----- FRAME MAIN NULL  ------ }  ------ 7
ADI R2 -16 ;====133
CMP R2 SL ;====145
BLT R2 OVERFLOW ;====157
MOV R6 FP ;====169
MOV FP SP ;====181
ADI SP -4 ;====193
STR R6 SP ;====205
ADI SP -4 ;====217
ADI SP -4 ;====229
MOV R2 PC ;====241  ;----- CALL MAIN
ADI R2 36 ;====253
STR R2 FP ;====265
JMP MAIN ;====277
JMP END ;====289  ;----- JMP END
Y101 ADI SP 0 ;====301  ;----- Y101 FUNC Y101
MOV SP FP ;====313  ;----- RETURN this  ------ iTree() {}  ------ 2
MOV R2 SP ;====325
CMP R2 SB ;====337
BGT R2 UNDERFLOW ;====349
LDR R3 FP ;====361
MOV R4 FP ;====373
ADI R4 -8 ;====385
LDR R5 R4 ;====397
ADI FP -4 ;====409
LDR FP FP ;====421
STR R5 SP ;====433
JMR R3 ;====445
X101 ADI SP 0 ;====457  ;----- X101 FUNC X101
MOV R2 SP ;====469  ;----- FRAME Y101 this
ADI R2 -12 ;====481
CMP R2 SL ;====493
BLT R2 OVERFLOW ;====505
MOV R6 FP ;====517
MOV FP SP ;====529
ADI SP -4 ;====541
STR R6 SP ;====553
ADI SP -4 ;====565
ADI R6 -8 ;====577
LDR R2 R6 ;====589
STR R2 SP ;====601
ADI SP -4 ;====613
MOV R2 PC ;====625  ;----- CALL Y101
ADI R2 36 ;====637
STR R2 FP ;====649
JMP Y101 ;====661
MOV SP FP ;====673  ;----- RETURN this  ------ }  ------ 3
MOV R2 SP ;====685
CMP R2 SB ;====697
BGT R2 UNDERFLOW ;====709
LDR R3 FP ;====721
MOV R4 FP ;====733
ADI R4 -8 ;====745
LDR R5 R4 ;====757
ADI FP -4 ;====769
LDR FP FP ;====781
STR R5 SP ;====793
JMR R3 ;====805
MAIN ADI SP -4 ;====817  ;----- MAIN FUNC MAIN  ------ void kxi2017 main() {  ------ 4
LDR R2 Btrue ;====829  ;----- EQ Btrue Btrue t100  ------ if(true==true){}  ------ 5
LDR R3 Btrue ;====841
CMP R2 R3 ;====853
BRZ R2 SKIP_2 ;====865
MOV R4 FP ;====877
ADI R4 -12 ;====889
STR R0 R4 ;====901
JMP SKIP_3 ;====913
SKIP_2 MOV R4 FP ;====925
ADI R4 -12 ;====937
STR R1 R4 ;====949
SKIP_3 MOV R4 FP ;====961  ;----- BF t100 SKIP_1 
ADI R4 -12 ;====973
LDR R2 R4 ;====985
BRZ R2 SKIP_1 ;====997
SKIP_1 MOV SP FP ;====1009  ;----- SKIP_1 RETURN this  ------ }  ------ 6
MOV R2 SP ;====1021
CMP R2 SB ;====1033
BGT R2 UNDERFLOW ;====1045
LDR R3 FP ;====1057
MOV R4 FP ;====1069
ADI R4 -8 ;====1081
LDR R5 R4 ;====1093
ADI FP -4 ;====1105
LDR FP FP ;====1117
STR R5 SP ;====1129
JMR R3 ;====1141
OVERFLOW LDB R3 H!
TRP 3
UNDERFLOW LDB R3 H!
TRP 3
END TRP 0