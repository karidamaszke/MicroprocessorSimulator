using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.Threading;


namespace uPSimulator
{

    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        // containers with orders

        public List<string> commands = new List<string> { };
        public List<string> operand1 = new List<string> { };
        public List<string> operand2 = new List<string> { };

        // all registers used by microprocessor
        Register RAH = new Register("AH", 0);
        Register RAL = new Register("AL", 0);
        Register RBH = new Register("BH", 0);
        Register RBL = new Register("BL", 0);
        Register RCH = new Register("CH", 0);
        Register RCL = new Register("CL", 0);
        Register RDH = new Register("DH", 0);
        Register RDL = new Register("DL", 0);

        // stack definition
        public static int M = 20;
        public int SP = M - 1;
        public short[] stack = new short[M];

        // variables which tells what kind of operation is executing
        public bool arythmetic_order = false;
        public bool stack_order = false;
        public bool interrupt_order = false;

        // helper variables
        public int number;
        public int step_index = 0;


        // main constructor
        public MainWindow()
        {
            InitializeComponent();
        }


        // choose which kind of operation should be save
        private void Apply_Command(object sender, RoutedEventArgs e)
        {
            if (arythmetic_order == true)
            {
                Arythmetic_Command();
            }
            else if (stack_order == true)
            {
                Stack_Command();
            }
            else if (interrupt_order == true)
            {
                Interrupt_Command();
            }
        }

        // normal mode
        private void Run(object sender, RoutedEventArgs e)
        {
            if (step.IsChecked == false)
            {
                int index = 0;
                foreach (var comm in commands)
                {
                    Action(index, comm);
                    index++;
                }
            }
        }

        // single stepping mode
        private void Single_Stepping(object sender, RoutedEventArgs e)
        {
            if (step_index == commands.Count())
            {
                Thread.Sleep(2000);
                step_index = 0;
                Clear_Registers(sender, e);
            }

            if (step_index == 0)
            {
                Clear_Registers(sender, e);
            }

            if (step.IsChecked == true)
            {
                instruction_number.Text = (step_index + 1).ToString();
                Action(step_index, commands[step_index]);
                step_index++;
            }
        }

        // choose registers for current arythmetic operation
        private void ChooseRegAr(ref Register r1, ref Register r2, int index)
        {
            List<Register> registers = new List<Register> { RAH, RAL, RBH, RBL, RCH, RCL, RDH, RDL };
            foreach (Register register in registers)
            {
                if (operand1[index] == register.name)
                {
                    r1 = register;
                }
                if (operand2[index] == register.name)
                {
                    r2 = register;
                }
            }

            // if second operand is a number
            if (r2.name == "2")
            {
                r2.value = Convert.ToByte(operand2[index], 16);
            }
        }

        //choose register for current stack operation
        private void ChooseRegSt(ref Register r1, ref Register r2, int index)
        {
            if (operand1[index] == "AX")
            {
                r1 = RAH;
                r2 = RAL;
            }
            else if (operand1[index] == "BX")
            {
                r1 = RBH;
                r2 = RBL;
            }
            else if (operand1[index] == "CX")
            {
                r1 = RCH;
                r2 = RCL;
            }
            else if (operand1[index] == "DX")
            {
                r1 = RDH;
                r2 = RDL;
            }

        }

        // executing current order
        private void Action(int index, string comm)
        {
            Register r1 = new Register("1", 0);
            Register r2 = new Register("2", 0);

            // choose operation
            switch (comm)
            {
                case "MOV":
                    ChooseRegAr(ref r1, ref r2, index);
                    r1.MOV(r2.value);
                    break;
                case "ADD":
                    ChooseRegAr(ref r1, ref r2, index);
                    r1.ADD(r2.value);
                    break;
                case "SUB":
                    ChooseRegAr(ref r1, ref r2, index);
                    r1.SUB(r2.value);
                    break;
                case "PUSH":
                    ChooseRegSt(ref r1, ref r2, index);
                    Push(r1, r2);
                    break;
                case "POP":
                    ChooseRegSt(ref r1, ref r2, index);
                    Pop(r1, r2);
                    break;
                case "INT":
                    Interrupt(index);
                    break;
            }
            RefreshRegisters();

        }


        // clearing all commands created at this session
        private void Clear_Commands(object sender, RoutedEventArgs e)
        {
            instruction_list.Text = "";
            instruction_number.Text = "";
            step_index = 0;
            commands.Clear();
            operand1.Clear();
            operand2.Clear();
        }

        // clearing all registers
        private void Clear_Registers(object sender, RoutedEventArgs e)
        {
            List<Register> registers = new List<Register> { RAH, RAL, RBH, RBL, RCH, RCL, RDH, RDL };
            foreach (Register register in registers)
            {
                register.value = 0;
            }
            RefreshRegisters();
            instruction_number.Text = "";
            step_index = 0;
        }


        // refreshing contents of register on the screen
        private void RefreshRegisters()
        {
            AH_VAL.Text = "0x" + RAH.value.ToString("X");
            AL_VAL.Text = "0x" + RAL.value.ToString("X");
            BH_VAL.Text = "0x" + RBH.value.ToString("X");
            BL_VAL.Text = "0x" + RBL.value.ToString("X");
            CH_VAL.Text = "0x" + RCH.value.ToString("X");
            CL_VAL.Text = "0x" + RCL.value.ToString("X");
            DH_VAL.Text = "0x" + RDH.value.ToString("X");
            DL_VAL.Text = "0x" + RDL.value.ToString("X");

        }

        // refresh stack list
        private void RefreshStack()
        {
            stack_list.Text = "";
            for (int i = 19; i > SP; i--)
            {
                stack_list.Text += "0x" + stack[i].ToString("X") + "\n";
            }
        }


        // saving code to file
        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("code.txt"))
                {
                    int index = 0;
                    foreach (var comm in commands)
                    {
                        sw.WriteLine(comm);
                        sw.WriteLine(operand1[index]);
                        sw.WriteLine(operand2[index]);
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The file could not be open");
            }
        }

        // loading code from file
        private void Load(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader("code.txt"))
                {
                    // all orders consist of three elements
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        commands.Add(line);
                        line = sr.ReadLine();
                        operand1.Add(line);
                        line = sr.ReadLine();
                        operand2.Add(line);
                    }
                }
            }
            catch
            {
                MessageBox.Show("The file could not be open");
            }

            // write the code on the instruction list
            try
            {
                int index = 0;
                string command = "";
                instruction_list.Text = "";

                foreach (var comm in commands)
                {
                    command += (index + 1).ToString() + ".\t" + comm + "\t" + operand1[index];
                    
                    // checking if operand is a register or a number
                    if (operand2[index] != "null")
                    {
                        try
                        {
                            int n = Convert.ToInt32(operand2[index]);
                            command += ", 0x" + operand2[index] + "\n";
                        }
                        catch
                        {
                            command += ", " + operand2[index] + "\n";
                        }
                    }                   
                    index++;
                }
                instruction_list.Text = command;
            }
            catch
            {
                MessageBox.Show("Error! Something went wrong");
            }
        }


        // create arythmetic command
        private void Arythmetic_Command()
        {
            string command = "";

            // checking which operation will be execute
            List<RadioButton> commandsRadioButtons = new List<RadioButton> { mov, add, sub };
            foreach (RadioButton radioButton in commandsRadioButtons)
            {
                if (((RadioButton)radioButton).IsChecked == true)
                {
                    string name = radioButton.Name.ToUpper();
                    command += (commands.LongCount() + 1).ToString() + ".\t" + name + "\t";
                    commands.Add(name);
                }
            }

            // checking number of first operand
            List<RadioButton> operand1RadioButtons = new List<RadioButton> { ah1, al1, bh1, bl1, ch1, cl1, dh1, dl1 };
            foreach (RadioButton radioButton in operand1RadioButtons)
            {
                if (((RadioButton)radioButton).IsChecked == true)
                {
                    string name = radioButton.Name.Substring(0, 2).ToUpper();
                    command += name + ", ";
                    operand1.Add(name);
                }
            }

            // checking number of second operand
            List<RadioButton> operand2RadioButtons = new List<RadioButton> { ah2, al2, bh2, bl2, ch2, cl2, dh2, dl2, num };
            foreach (RadioButton radioButton in operand2RadioButtons)
            {
                if (((RadioButton)radioButton).IsChecked == true)
                {
                    string name = radioButton.Name.Substring(0, 2).ToUpper();

                    if (name == "NU")
                    {
                        try
                        {
                            Convert.ToByte(x.Text, 16);
                            command += "0x" + x.Text;
                            operand2.Add(x.Text);
                        }
                        catch
                        {
                            MessageBox.Show("Number should be one byte!");
                            command += "0xFF";
                            operand2.Add("FF");
                        }
                    }
                    else
                    {
                        command += name;
                        operand2.Add(name);
                    }

                }
            }

            instruction_list.Text += command + "\n";
        }

        // create stack command
        private void Stack_Command()
        {
            string command = "";

            // checking which operation will be execute
            List<RadioButton> commandsRadioButtons = new List<RadioButton> { push, pop };
            foreach (RadioButton radioButton in commandsRadioButtons)
            {
                if (((RadioButton)radioButton).IsChecked == true)
                {
                    string name = radioButton.Name.ToUpper();
                    command += (commands.LongCount() + 1).ToString() + ".\t" + name + "\t";
                    commands.Add(name);
                }
            }

            // checking which register was chosen
            List<RadioButton> registerRadioButtons = new List<RadioButton> { ax, bx, cx, dx };
            foreach (RadioButton radioButton in registerRadioButtons)
            {
                if (((RadioButton)radioButton).IsChecked == true)
                {
                    string name = radioButton.Name.ToUpper();
                    command += name;
                    operand1.Add(name);
                }
            }
            operand2.Add("null");

            instruction_list.Text += command + "\n";
        }

        // create interrupt command
        private void Interrupt_Command()
        {
            string command = (commands.LongCount() + 1).ToString() + ".\t" + "INT 0x21" + "\t";
            commands.Add("INT");
            operand1.Add("null");
            operand2.Add("null");
            instruction_list.Text += command + "\n";
        }


        // stack functions
        private void Push(Register old, Register young)
        {
            if (SP == 0)
            {
                MessageBox.Show("Stack overflow!");
            }
            else
            {
                stack[SP] = (short)(young.value | (old.value << 8));
                SP -= 1;
            }
            RefreshStack();
        }

        private void Pop(Register old, Register young)
        {
            if (SP == 19)
            {
                MessageBox.Show("Stack is empty!");
            }
            else
            {
                SP += 1;
                young.value = (byte)(stack[SP] & 0xff);
                old.value = (byte)((stack[SP] & 0xff00) >> 8);
            }
            RefreshStack();
        }


        // interrupts
        private void Interrupt(int index)
        {
            switch (RAH.value)
            {
                // terminate program
                case 0:
                    this.Close();
                    break;
                // run command terminal
                case 1:
                    Process.Start("cmd.exe");
                    break;
                // display char from DL register
                case 2:
                    MessageBox.Show(Encoding.ASCII.GetString(new byte[] { RDL.value }));
                    break;
                // get current disk number
                case 25:
                    RAL.value = (byte)(char.ToUpper(Path.GetPathRoot(Environment.CurrentDirectory)[0]));
                    break;
                // get current date
                case 42:
                    var date = DateTime.Today;
                    RAL.value = (byte)date.DayOfWeek;
                    RDL.value = (byte)date.Day;
                    RDH.value = (byte)date.Month;
                    RCL.value = (byte)(date.Year & 0xff);
                    RCH.value = (byte)((date.Year & 0xff00) >> 8);
                    break;
                // get current time
                case 44:
                    var time = DateTime.Now.TimeOfDay;
                    RDL.value = (byte)time.Milliseconds;
                    RDH.value = (byte)time.Seconds;
                    RCL.value = (byte)time.Minutes;
                    RCH.value = (byte)time.Hours;
                    break;
                // get os version
                case 48:
                    var os = Environment.OSVersion;
                    MessageBox.Show("OS version: " + os.VersionString);
                    RAL.value = (byte)os.Version.Major;
                    RAH.value = (byte)os.Version.Minor;
                    break;
                // clear code.txt file
                case 237:
                    try
                    {
                        File.WriteAllText("code.txt", String.Empty);
                        RAL.value = 1;
                    }
                    catch
                    {
                        RAL.value = 14;
                    }
                    break;
            }

        }


        // functions switching panels
        private void mov_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Visible;
            operand2_panel.Visibility = Visibility.Visible;
            stack_register.Visibility = Visibility.Hidden;
            arythmetic_order = true;
            stack_order = false;
            interrupt_order = false;
        }

        private void add_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Visible;
            operand2_panel.Visibility = Visibility.Visible;
            stack_register.Visibility = Visibility.Hidden;
            arythmetic_order = true;
            stack_order = false;
            interrupt_order = false;
        }

        private void sub_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Visible;
            operand2_panel.Visibility = Visibility.Visible;
            stack_register.Visibility = Visibility.Hidden;
            arythmetic_order = true;
            stack_order = false;
            interrupt_order = false;
        }

        private void int_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Hidden;
            operand2_panel.Visibility = Visibility.Hidden;
            stack_register.Visibility = Visibility.Hidden;
            arythmetic_order = false;
            stack_order = false;
            interrupt_order = true;
        }

        private void push_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Hidden;
            operand2_panel.Visibility = Visibility.Hidden;
            stack_register.Visibility = Visibility.Visible;
            arythmetic_order = false;
            stack_order = true;
            interrupt_order = false;
        }

        private void pop_Checked(object sender, RoutedEventArgs e)
        {
            operand1_panel.Visibility = Visibility.Hidden;
            operand2_panel.Visibility = Visibility.Hidden;
            stack_register.Visibility = Visibility.Visible;
            arythmetic_order = false;
            stack_order = true;
            interrupt_order = false;
        }

        private void help_window(object sender, RoutedEventArgs e)
        {
            InterruptHelp help_window = new InterruptHelp();
            help_window.Show();
        }
    }
}
