using System;

namespace uPSimulator
{
    class Register
    {
        public string name;
        public byte value;

        // constructor
        public Register(string name, byte value)
        {
            this.name = name;
            this.value = value;
        }

        // copy constructor
        public Register(Register r)
        {
            this.name = r.name;
            this.value = r.value;
        }

        public void MOV(byte operand2)
        {
            this.value = operand2;            
        }

        public void ADD(byte operand2)
        {
            if ((this.value + operand2) > 255)
            {
                this.value = 255;
            }
            else
            {
                this.value += operand2;
            }
        }

        public void SUB(byte operand2)
        {
            this.value = Convert.ToByte(Math.Abs(this.value - operand2));
        }

    }
}
