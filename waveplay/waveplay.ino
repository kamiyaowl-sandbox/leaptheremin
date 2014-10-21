#include<SPI.h>

#define CHANGE_FREQ 0x1
#define CHANGE_VOL 0x2
#define CHANGE_POLT 0x3

int speakerPin = 3;
int csPin = 10;//AD5263(Digital Potentio Meter)

uint16_t freq = 440;
uint16_t output_freq = 440;
//
uint16_t freq_buf = 0x0;
uint8_t vol_addr = 0x0;
uint8_t vol_value = 0xff;

uint8_t poltament_val = 0x1;

uint8_t recv_count = 0x0;
uint8_t instruction = 0x0;


void setup(){
  SPI.begin();
  SPI.setBitOrder(MSBFIRST);
  SPI.setClockDivider(SPI_CLOCK_DIV4);
  
  Serial.begin(115200);

  pinMode(speakerPin, OUTPUT);
  pinMode(csPin, OUTPUT);
  digitalWrite(csPin,HIGH);
  
  /* Wakeup Test */
  vol_value = 0xff;
  volume_update();
  tone(speakerPin,freq);
  for(freq = 31 ; freq < 16383 ; ++freq){
    tone(speakerPin,freq);
    delayMicroseconds(100);
  }
  freq = 440;
  tone(speakerPin,freq);
  delay(200);
  vol_value = 0x0;
  volume_update();
  delay(200);
  vol_value = 0xff;
  volume_update();
  tone(speakerPin,freq);
  delay(200);
  vol_value = 0x0;
  volume_update();
  delay(200);

}

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
          case CHANGE_POLT:
            poltament_val = data & 0x7f;
           break;
       }
       recv_count++;
     }
   }
  tone_update();
}
void volume_update(){
  digitalWrite(csPin,LOW);
  SPI.transfer(vol_addr);
  SPI.transfer(vol_value);
  digitalWrite(csPin,HIGH);
}
void tone_update(){
  if(poltament_val) {
    if(output_freq > freq) {
      if(output_freq - freq > poltament_val){
          output_freq -= poltament_val;
      } else {
        output_freq = freq;
      }
    } else if(output_freq < freq){
      if(freq - output_freq > poltament_val){
        output_freq += poltament_val;
      } else {
        output_freq = freq;
      }
    }
  } else {
    output_freq = freq;
  }
  tone(speakerPin,output_freq);
}
