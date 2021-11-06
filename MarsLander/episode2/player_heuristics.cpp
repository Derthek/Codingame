#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

int limit_rotation(int rotate){
    return max(min(rotate,45),-45);
}
int limit_power(int power){
    return max(min(power,4),2);
}
bool above_flat(int X, int X1, int X2){
    if(X >= X1 && X <= X2){
        return true;
    }
    return false;
}
bool has_hSpeed(int hSpeed){
    if(abs(hSpeed) > 1) {
        return true;
    } else {
        return false;
    }
}
void go_to_flat(int flatX1, int flatX2,int power,int rotate, int X){
    if ( X > flatX1){
        rotate += 15;
    }else if ( X < flatX2){
        rotate -= 15;
    }
    power++;
    cout << limit_rotation(rotate) << " " << limit_power(power) << endl;
}
void stop_hSpeed(int hSpeed, int power, int rotate){
    if(hSpeed < 0) {
        rotate -= 15;
    }else if(hSpeed > 0){
        rotate += 15;
    }
    power++;
    cout << limit_rotation(rotate) << " " << limit_power(power) << endl;
}
void land(int vSpeed, int power, int rotate){
    if(vSpeed < -40){
        power++;
    }
    cout << limit_rotation(rotate) << " " << limit_power(power) << endl;
}
int main()
{
    int surfaceN;
    cin >> surfaceN; cin.ignore();
    int prevY = -1;
    int prevX = -1;
    int flatY = -1;
    int flatX1 = -1;
    int flatX2 = -2;
    for (int i = 0; i < surfaceN; i++) {
        int landX;
        int landY;
        cin >> landX >> landY; cin.ignore();
        if(prevY == landY){
            flatX1 = prevX;
            flatX2 = landX;
            flatY = landY;
        }
        prevY = landY;
        prevX = landX;
    }
    cerr << flatX1 << " " << flatX2 << endl;
    while (1) {
        int X;
        int Y;
        int hSpeed;
        int vSpeed;
        int fuel;
        int rotate;
        int power;
        cin >> X >> Y >> hSpeed >> vSpeed >> fuel >> rotate >> power; cin.ignore();

        // Write an action using cout. DON'T FORGET THE "<< endl"
        // To debug: cerr << "Debug messages..." << endl;
        if(above_flat(X,flatX1,flatX2)){
            if(has_hSpeed(hSpeed)){
                stop_hSpeed(hSpeed, power, rotate);
            }else{
                land(vSpeed, power, rotate);
            }
        }else {
            go_to_flat(flatX1,flatX2,power,rotate, X);
        }
    }
}