#define keycnt 3

short analogpins[] = { 0, 18, 19 };
short digitalpins[] = { 0, 7, 10 };
short notes[] = {-1, 62, 63};
bool active[]={0,0,0};
short lastpitch[]={-1,-1,-1};

bool handshaked = false;



void setup() {
  pinMode(13, OUTPUT);
  pinMode(3, OUTPUT);
  for (short i = 1; i < keycnt; ++i) pinMode(digitalpins[i], INPUT);
  Serial.begin(9600);
}

void loop() {
   if (!handshaked)
    {
        if (Serial.available())
        {
            String cmd = Serial.readStringUntil('\n');
            cmd.trim();

            if (cmd == "connect?")
            {
                Serial.println("ardboardready");
                handshaked = true;
            }
        }
        return;
    }
    else{
    
    for (int i = 1; i < keycnt; ++i) {
      digitalWrite(13,HIGH);
      digitalWrite(3,HIGH);
      if (digitalRead(digitalpins[i]) == HIGH) {
        if(active[i]!=1){
          short t = analogRead(analogpins[i]);
          t= map(t,0,1023, 0, 16383);
          active[i] = 1;
          lastpitch[i]=t;
          Serial.println("on,"+String(notes[i])+','+String(t));
        }else{
          short t = analogRead(analogpins[i]);
          t= map(t,0,1023, 0, 16383);
          if(t!=lastpitch[i]){
            lastpitch[i]=t;
            Serial.println("pitch,"+String(notes[i])+','+String(t));
          }
        }

      }else if(active[i]==1){
        Serial.println("off,"+String(notes[i])); 
        active[i]=0;  
        lastpitch[i]=-1;
      }
      delay(10);
    }
  }
}



