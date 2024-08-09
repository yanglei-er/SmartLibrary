#include <SoftwareSerial.h>
SoftwareSerial uno2(52,53); //RX, TX

//电机设置
#define right_IN1 22
#define right_IN2 23
#define right_IN3 24
#define right_IN4 25
#define right_ENA 6
#define right_ENB 5
#define left_IN1 26
#define left_IN2 27
#define left_IN3 28
#define left_IN4 29
#define left_ENA 8
#define left_ENB 7

// 循迹设置 前进
#define forward_leftA_track_PIN 13
#define forward_leftB_track_PIN 12
#define forward_middle_track_PIN 11
#define forward_rightA_track_PIN 10
#define forward_rightB_track_PIN 9
//返回
#define return_leftA_track_PIN 42
#define return_leftB_track_PIN 44
#define return_middle_track_PIN 46
#define return_rightA_track_PIN 48
#define return_rightB_track_PIN 50
//传感器状态
int sensor[5] = {0, 0, 0, 0, 0};
//循迹参数
float Kp = 10, Ki = 0.5, Kd = 0;                    //pid弯道参数参数 
float error = 0, P = 0, I = 0, D = 0, PID_value = 0;//pid直道参数 
float previous_error = 0, previous_I = 0;           //误差值 
static int initial_motor_speed = 80;               //初始速度
//转弯参数
#define big_left -4
#define small_left -2
#define stright 0
#define small_right 2
#define big_right 4
#define turn_max 255
#define turn_min 20
#define delay_time 50

void read_sensor_values_forward();  //前进读取巡线
void read_sensor_values_return(); //返回读取巡线
void calc_pid();  //计算pid 
void motor_control_forward(); //电机控制
void motor_control_return(); //电机控制

String state = "stop";
bool is_turn = false;
int times = 0;

void setup()
{
  Serial.begin(9600);
  uno2.begin(9600);
  //监听uno2
  uno2.listen();
  // 循迹引脚初始化 前进
  pinMode (forward_leftA_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (forward_leftB_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (forward_middle_track_PIN, INPUT);//设置引脚为输入引脚
  pinMode (forward_rightA_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (forward_rightB_track_PIN, INPUT); //设置引脚为输入引脚
  //返回
  pinMode (return_leftA_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (return_leftB_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (return_middle_track_PIN, INPUT);//设置引脚为输入引脚
  pinMode (return_rightA_track_PIN, INPUT); //设置引脚为输入引脚
  pinMode (return_rightB_track_PIN, INPUT); //设置引脚为输入引脚

  // 电控引脚初始化
  pinMode (left_IN1, OUTPUT); //设置引脚为输出引脚
  pinMode (left_IN2, OUTPUT); //设置引脚为输出引脚
  pinMode (left_IN3, OUTPUT); //设置引脚为输出引脚
  pinMode (left_IN4, OUTPUT); //设置引脚为输出引脚
  pinMode (left_ENA, OUTPUT); //设置引脚为输出引脚
  pinMode (left_ENB, OUTPUT); //设置引脚为输出引脚
  pinMode (right_IN1, OUTPUT); //设置引脚为输出引脚
  pinMode (right_IN2, OUTPUT); //设置引脚为输出引脚
  pinMode (right_IN3, OUTPUT); //设置引脚为输出引脚
  pinMode (right_IN4, OUTPUT); //设置引脚为输出引脚
  pinMode (right_ENA, OUTPUT); //设置引脚为输出引脚
  pinMode (right_ENB, OUTPUT); //设置引脚为输出引脚
}

void loop()
{
  if(uno2.available())
  {
    String data = uno2.readString();
    Serial.println(data);
    if(data == "start")
    {
      //更新状态为satrt
      state = "start";
      //start命令则启动前往
      motorStart();
    }
    else if(data == "turn")
    {
      //turn启用转弯
      is_turn = true;
    }
    else if(data == "return")
    {
      //更新状态为return
      state = "return";
      //禁止转弯
      is_turn = false;
      //return则启动返回
      motorReturn();
    }
  }

  if(state == "stop")
  {
    delay(500);
  }
  else if(state == "start")
  {
    read_sensor_values_forward(); //读取传感器值
    calc_pid(); //计算PID
    motor_control_forward();  //驱动电机
    delay(delay_time);
  }
  else if(state == "return")
  {
    read_sensor_values_return();//读取传感器值
    calc_pid();//计算PID
    motor_control_return();//驱动电机
    delay(delay_time);
  }
  else if(state == "arrived")
  {
    motorsStop();
    uno2.print("arrived");
    state = "stop";
  }
  else if(state == "over")
  {
    motorsStop();
    uno2.print("over");
    state = "stop";
  }
}

void read_sensor_values_forward()
{
  sensor[0] = digitalRead(forward_leftA_track_PIN);
  sensor[1] = digitalRead(forward_leftB_track_PIN);
  sensor[2] = digitalRead(forward_middle_track_PIN);
  sensor[3] = digitalRead(forward_rightA_track_PIN);
  sensor[4] = digitalRead(forward_rightB_track_PIN);

  if((sensor[0] == 1) && (sensor[1] == 0) && (sensor[2] == 0) && (sensor[3] == 0) && (sensor[4] == 1))
  {//                     1 0 0 0 1 停车
    error = 0;
    state = "arrived";
  }
  else if((sensor[0] == 0) && (sensor[1] == 0) && (sensor[2] == 0) && (sensor[3] == 0) && (sensor[4] == 0))
  {//                     0 0 0 0 0 停车
    error = 0;
    state = "arrived";
  }
  // 转向
  else if(((sensor[0] == 0) || (sensor[1] == 0)) && is_turn && times < 5)
  {
    error = small_left;
    times++;
  }
  else if((sensor[0] == 1) && (sensor[1] == 1) && (sensor[2] == 1) && (sensor[3] == 1) && (sensor[4] == 1))
  {//                      1 1 1 1 1按原来走
    error = error;
  }
  else if (sensor[4] == 0)
  {
    error = big_right;//          1 1 1 1 0 大右转
  }
  else if (sensor[3] == 0)
  {
    error = small_right;//          1 1 1 0 1 小右转
  }
  else if (sensor[2] == 0)
  {
    error = stright;//          1 1 0 1 1 直走
  }
  else if (sensor[1] == 0)
  {
    error = small_left;//         1 0 1 1 1 小左转
  }
  else if (sensor[0] == 0)
  {
    error = big_left;//         0 1 1 1 1 大左转
  }
}

void read_sensor_values_return()
{
  sensor[0] = digitalRead(return_leftA_track_PIN);
  sensor[1] = digitalRead(return_leftB_track_PIN);
  sensor[2] = digitalRead(return_middle_track_PIN);
  sensor[3] = digitalRead(return_rightA_track_PIN);
  sensor[4] = digitalRead(return_rightB_track_PIN);

  if((sensor[0] == 1) && (sensor[1] == 0) && (sensor[2] == 0) && (sensor[3] == 0) && (sensor[4] == 1))
  {//                     1 0 0 0 1 停车
    error = 0;
    state = "over";
  }
  else if((sensor[0] == 0) && (sensor[1] == 0) && (sensor[2] == 0) && (sensor[3] == 0) && (sensor[4] == 0))
  {//                     0 0 0 0 0 停车
    error = 0;
    state = "over";
  }
  else if((sensor[0] == 1) && (sensor[1] == 1) && (sensor[2] == 1) && (sensor[3] == 1) && (sensor[4] == 1))
  {//                      1 1 1 1 1按原来走
    error = error;
  }
  else if (sensor[4] == 0)
  {
    error = big_right;//          1 1 1 1 0 大右转
  }
  else if (sensor[3] == 0)
  {
    error = small_right;//          1 1 1 0 1 小右转
  }
  else if (sensor[2] == 0)
  {
    error = stright;//          1 1 0 1 1 直走
  }
  else if (sensor[1] == 0)
  {
    error = small_left;//         1 0 1 1 1 小左转
  }
  else if (sensor[0] == 0)
  {
    error = big_left;//         0 1 1 1 1 大左转
  }
}

void calc_pid()
{
  P = error;
  I = I + error;
  D = error - previous_error;
 
  PID_value = (Kp * P) + (Ki * I) + (Kd * D);
 
  previous_error = error;
}

void motor_control_forward()
{
  int left_motor_speed = initial_motor_speed - PID_value;
  int right_motor_speed = initial_motor_speed + PID_value;

  if(error == 0)
  {
    if(left_motor_speed > initial_motor_speed)
    {
      left_motor_speed = left_motor_speed - 1;
    }
    else
    {
      left_motor_speed = left_motor_speed + 1;
    }
    if(right_motor_speed > initial_motor_speed)
    {
      right_motor_speed = right_motor_speed - 1;
    }
    else
    {
      right_motor_speed = right_motor_speed + 1;
    }
  }
  
  if(left_motor_speed < turn_min)
  {
    left_motor_speed = turn_min;
  }
  else if(left_motor_speed > turn_max)
  {
    left_motor_speed = turn_max;
  }
  if(right_motor_speed < turn_min)
  {
    right_motor_speed = turn_min;
  }
  else if(right_motor_speed > turn_max)
  {
    right_motor_speed = turn_max;
  }

  Serial.println(left_motor_speed);
  Serial.println(right_motor_speed);
  analogWrite(left_ENA, right_motor_speed);
  analogWrite(left_ENB, right_motor_speed);
  analogWrite(right_ENA, left_motor_speed);
  analogWrite(right_ENB, left_motor_speed);

  // 输出
  if(error == stright)
  {
    Serial.println("直行"); 
  }
  else if(error == small_right)
  {
    Serial.println("小右转");
  }
  else if(error == big_right)
  {
    Serial.println("大右转");
  }
  else if(error == small_left)
  {
    Serial.println("小左转");
  }
  else if(error == big_left)
  {
    Serial.println("大左转");
  }
  else
  {
    Serial.println("停车");
  }
}

void motor_control_return()
{
  int left_motor_speed = initial_motor_speed - PID_value;
  int right_motor_speed = initial_motor_speed + PID_value;

  if(error == 0)
  {
    if(left_motor_speed > initial_motor_speed)
    {
      left_motor_speed = left_motor_speed - 1;
    }
    else
    {
      left_motor_speed = left_motor_speed + 1;
    }
    if(right_motor_speed > initial_motor_speed)
    {
      right_motor_speed = right_motor_speed - 1;
    }
    else
    {
      right_motor_speed = right_motor_speed + 1;
    }
  }
  
  if(left_motor_speed < turn_min)
  {
    left_motor_speed = turn_min;
  }
  else if(left_motor_speed > turn_max)
  {
    left_motor_speed = turn_max;
  }
  if(right_motor_speed < turn_min)
  {
    right_motor_speed = turn_min;
  }
  else if(right_motor_speed > turn_max)
  {
    right_motor_speed = turn_max;
  }

  Serial.println(left_motor_speed);
  Serial.println(right_motor_speed);
  analogWrite(left_ENA, left_motor_speed);
  analogWrite(left_ENB, left_motor_speed);
  analogWrite(right_ENA, right_motor_speed);
  analogWrite(right_ENB, right_motor_speed);

  // 输出
  if(error == stright)
  {
    Serial.println("直行"); 
  }
  else if(error == small_right)
  {
    Serial.println("小右转");
  }
  else if(error == big_right)
  {
    Serial.println("大右转");
  }
  else if(error == small_left)
  {
    Serial.println("小左转");
  }
  else if(error == big_left)
  {
    Serial.println("大左转");
  }
  else
  {
    Serial.println("停车");
  }
}

void motorStart()
{
  digitalWrite(left_IN1, 1);
  digitalWrite(left_IN2, 0);
  digitalWrite(left_IN3, 0);
  digitalWrite(left_IN4, 1);
  digitalWrite(right_IN1, 0);
  digitalWrite(right_IN2, 1);
  digitalWrite(right_IN3, 0);
  digitalWrite(right_IN4, 1);
  analogWrite(left_ENA, 0);
  analogWrite(left_ENB, 0);
  analogWrite(right_ENA, 0);
  analogWrite(right_ENB, 0);
}

void motorReturn()
{
  digitalWrite(left_IN1, 0);
  digitalWrite(left_IN2, 1);
  digitalWrite(left_IN3, 1);
  digitalWrite(left_IN4, 0);
  digitalWrite(right_IN1, 1);
  digitalWrite(right_IN2, 0);
  digitalWrite(right_IN3, 1);
  digitalWrite(right_IN4, 0);
  analogWrite(left_ENA, 0);
  analogWrite(left_ENB, 0);
  analogWrite(right_ENA, 0);
  analogWrite(right_ENB, 0);
}

void motorsStop() 
{
  digitalWrite(left_IN1, 0);
  digitalWrite(left_IN2, 0);
  digitalWrite(left_IN3, 0);
  digitalWrite(left_IN4, 0);
  digitalWrite(right_IN1, 0);
  digitalWrite(right_IN2, 0);
  digitalWrite(right_IN3, 0);
  digitalWrite(right_IN4, 0);
  analogWrite(left_ENA, 0);
  analogWrite(left_ENB, 0);
  analogWrite(right_ENA, 0);
  analogWrite(right_ENB, 0);

  error = 0, P = 0, I = 0, D = 0, PID_value = 0; 
  previous_error = 0, previous_I = 0, times = 0;
}
