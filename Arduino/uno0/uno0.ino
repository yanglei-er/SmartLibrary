//舵机设置
#include <Servo.h>
Servo servo;
#define SERVO_PIN 13

void setup()
{
  //舵机归位
  servo.attach(SERVO_PIN);
  servo.write(5);

  Serial3.begin(9600);
}

void loop() 
{
  if(Serial3.available())
  {
    String data = Serial3.readString();
    if(data == "servo_turn")
    {
      //执行转动命令
      servocontrol();
    }
  }
  delay(200);
}

void servocontrol()
{
  for(int angle = 0; angle <=40; angle++)
  {
    servo.write(5+angle);
    delay(20);
  }
  delay(4000);
  for (int angle = 0; angle <=40; angle++)
  {
    servo.write(45-angle);
    delay(20);
  }
}
