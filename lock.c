int Flag = 0;
int lock(){
    int result;
    __asm{
        mov eax, 1
        xchg eax, Flag
        mov result,eax
    }
    return !result;
}

while (!lock()) { void; }
//кс
Flag = 0;

int turn;//номер процесса, чей код
int interested[N];//true, если проц. хочет занять кс
void enter_region(int proc){//id
    int other = 1 - proc; //номер конкурента
    interested[proc] = true;//говорит, что наш ход
    turn = proc;//глобальная!!!
    while (turn == proc && interested[other]){ void; }
    interested[proc] = false;
}