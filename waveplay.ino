#include<SPI.h>

#define CHANGE_FREQ 0x1
#define CHANGE_VOL 0x2

int speakerPin = 3;
int csPin = 10;

uint16_t freq = 440;
void setup(){
  SPI.begin();
  SPI.setBitOrder(MSBFIRST);
  SPI.setClockDivider(SPI_CLOCK_DIV4);
  
  Serial.begin(115200);

  pinMode(speakerPin, OUTPUT);
  pinMode(csPin, OUTPUT);
  digitalWrite(csPin,HIGH);
  
  volume_update();
}
uint16_t freq_buf = 0x0;
uint8_t vol_addr = 0x0;
uint8_t vol_value = 0xff;
uint8_t recv_count = 0x0;
uint8_t instruction = 0x0;

void loop(){
   while(Serial.available()){
     uint8_t data = Serial.read();
     if(data & 0x80){
       instruction = data & 0x7f;
       recv_count = 0;
     } else {
       switch(instruction){
         case CHANGE_FREQ:
           if(recv_count == 0){
             freq_buf &= 0x007f;
             freq_buf |= ((data & 0x7f) << 7);
           } else if(recv_count == 1){
             freq_buf &= 0xff80;
             freq_buf |= (data & 0x7f);
             freq = freq_buf;
           }
           break;
         case CHANGE_VOL:
            if(recv_count == 0){
             vol_addr = data & 0x7f;
           } else if(recv_count == 1){
             vol_value = (data & 0x7f) << 1;
             volume_update();
           }
           break;
       }
       recv_count++;
     }
   }
  tone(speakerPin,freq);
}
void volume_update(){
  digitalWrite(csPin,LOW);
  SPI.transfer(vol_addr);
  SPI.transfer(vol_value);
  digitalWrite(csPin,HIGH);
}
