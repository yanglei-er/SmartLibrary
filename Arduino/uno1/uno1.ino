#include <SoftwareSerial.h>
SoftwareSerial ble(4, 5); // (RX, TX)
SoftwareSerial barcode(9, 6); //RX, TX
SoftwareSerial uno2(2, 3); //RX, TX
SoftwareSerial uno0(12, 13); //RX, TX

#define bar_trig 10

void setup()
{
  pinMode(bar_trig, OUTPUT);
  digitalWrite(bar_trig, HIGH);

  Serial.begin(9600);
  barcode.begin(9600);
  ble.begin(9600);
  uno2.begin(9600);
  //首先监听蓝牙
  ble.listen();
}

void loop()
{
  if(ble.available())
  {
    String bleData = ble.readString();
    Serial.println(bleData);
    if(bleData == "scan")
    {
      //scan监听扫描器
      barcode.listen();
      digitalWrite(bar_trig, LOW);
    }
    else if(bleData.startsWith("带我去"))
    {
      //发送带我去至uno2，并监听uno2
      uno2.print(bleData);
      uno2.listen();
    }
  }

  if(barcode.available())
  {
    digitalWrite(bar_trig, HIGH);  
    String isbn = barcode.readString();
    ble.print(isbn);
    Serial.println(isbn);
    //扫描完成，再次监听蓝牙
    ble.listen();
  }

  if(uno2.available())
  {
    String data = uno2.readString();
    if(data == "servo_turn")
    {
      //执行转动命令
      uno0.print("servo_turn");
      //发送返回指令至uno2
      delay(6000);
      uno2.print("return");
    }
    else if(data == "over")
    {
      //成功返回起点,发送指令至客户端，重新监听蓝牙
      ble.print("over");
      ble.listen();
    }
  }
  delay(200);
}
