﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatsappSpammer;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using System.Management.Automation;
using System.Threading;
using MixERP.Net.VCards;

namespace WhatsAppSpammer
{
    public partial class WhatsAppSpammer : Form
    {

        private MessageSender ms { get; set; }
        private AppiumDevice appium;
        private bool settingsChanged;
        private Registrator5SIM smsActivate;
        private List<Device> devices;
        CancellationTokenSource automatizationTokenSource;
        private int Count { get; set; }
        public bool SettingsChanged
        {
            get
            {
                
                return SettingsChanged;
                
            }
            set
            {
                settingsChanged = value;
                buttonChangeAndSaveSettings.Enabled = value;
            }
        }
        public WhatsAppSpammer()
        {

            InitializeComponent();
            devices = new List<Device>();
            textBoxURL.Text = Properties.Settings.Default.URL;
            textBoxToken.Text = Properties.Settings.Default.Token;
            textBoxPath.Text = Properties.Settings.Default.PathToDirectory;
            SettingsChanged = false;
            string advs = CommandExecutor.ExecuteCommandSync("emulator -list-avds");
            string[] adv = advs.Split('\n');
            foreach (var item in adv)
            {
                comboBoxAppium.Items.Add(item);
            }
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/eula_accept");
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/registration_phone");
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/registration_submit");
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/registration_cc");
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/registration_country");
            comboBoxWhatsAppIds.Items.Add("com.whatsapp:id/verify_sms_code_input");
            //com.whatsapp:id/eula_accept 
            //com.whatsapp:id/registration_phone
            //com.whatsapp:id/registration_submit
            //com.whatsapp:id/registration_cc
            //	android:id/button1
            //com.whatsapp:id/verify_sms_code_input
            string ApiKey = "a13163e0458348e4b47f62bf8f440350";
            string Referal = "424401";
            string Country = "russia"; // "2";kazakhstan
            smsActivate = new Registrator5SIM(ApiKey, Referal, Country);

            textBoxName.Text = "Nickname";

            string response = ApiRequest.GetRequestSync("https://www.proxy-list.download/api/v1/get?type=http&country=RU");
            string[] adresses = response.Split('\n');
            Random rand = new Random();
            int index = rand.Next(0, adresses.Length);
            textBoxProxy.Text = adresses[index].Trim();

            ms = new MessageSender(Properties.Settings.Default.URL, Properties.Settings.Default.Token, Properties.Settings.Default.PathToDirectory);
            var numberBases = ms.GetNumberBasesInDirectory();
            foreach (var item in numberBases)
            {
                listBoxPhoneBases.Items.Add(item);
            }
            var selectedNumberBases = ms.GetNumberBases();
            foreach (var item in selectedNumberBases)
            {
                listBoxSelectedPhoneBases.Items.Add(item);
            }
        }

        private void buttonpath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog();
                textBoxPath.Text = dialog.SelectedPath;
                ms.ChangeFileReaderPath(textBoxPath.Text);
            }

        }

        private void textBoxURL_TextChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
        }

        private void textBoxToken_TextChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
        }

        private void textBoxPath_TextChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
        }
        private void changeAndSaveSettings()
        {
            Properties.Settings.Default.URL = textBoxURL.Text;
            Properties.Settings.Default.Token = textBoxToken.Text;
            Properties.Settings.Default.PathToDirectory = textBoxPath.Text;
            ms.Url = Properties.Settings.Default.URL;
            ms.Token = Properties.Settings.Default.Token;
            ms.ChangeFileReaderPath(Properties.Settings.Default.PathToDirectory);
            Properties.Settings.Default.Save();
        }
        private void buttonTest_Click(object sender, EventArgs e)
        {
            Count = 0;
            var numberbases = ms.GetNumberBases().Values;
            List<string> numbers = new List<string> { };
            foreach (var item in numberbases)
            {
                Count += item.Numbers.Count;
                numbers = numbers.Concat(item.Numbers).ToList();
            }
            progressBar.Value = 0;
            progressBar.Maximum = Count;

            string message = richTextBoxMessage.Text;
            sendMessageToSelectedPhoneBases(message);
        }
        private async void sendMessageToSelectedPhoneBases(string message)
        {
            buttonSendMessages.Enabled = false;
            foreach (NumberBase numberBase in ms.GetNumberBases().Values)
            {
                foreach (string number in numberBase.Numbers)
                {
                    listBoxServerAnswers.Items.Add(await Task.Run(() => ms.sendMessageAsync(number, message)));

                    progressBar.Value++;
                }
            }
            buttonSendMessages.Enabled = true;
        }
        private void listBoxPhoneBases_DoubleClick(object sender, EventArgs e)
        {
            int index = this.listBoxPhoneBases.SelectedIndex;
            if (index >= 0)
            {
                string filename = this.listBoxPhoneBases.Text;
                listBoxPhoneBases.Items.RemoveAt(index);
                ms.AddNumberBase(filename);
                listBoxSelectedPhoneBases.Items.Add(filename);
            }
        }

        private void listBoxSelectedPhoneBases_Click(object sender, EventArgs e)
        {

        }

        private void listBoxSelectedPhoneBases_DoubleClick(object sender, EventArgs e)
        {
            int index = this.listBoxSelectedPhoneBases.SelectedIndex;
            if (index >= 0)
            {
                string filename = this.listBoxSelectedPhoneBases.Text;
                listBoxSelectedPhoneBases.Items.RemoveAt(index);
                ms.RemoveNumberBase(filename);
                listBoxPhoneBases.Items.Add(filename);
            }
        }

        private void buttonChangeAndSaveSettings_Click(object sender, EventArgs e)
        {
            changeAndSaveSettings();
            SettingsChanged = false;
        }

        private async void buttontTest_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            List<string> numbers = new List<string>();
            var numberBases = ms.GetNumberBases();
            foreach (var numberBase in numberBases.Values)
            {
                numbers = numbers.Concat(numberBase.Numbers).ToList();
            }
            foreach(var number in numbers)
            {
                int iterations = 0;
                while (iterations < 60)
                {
                    try
                    {
                        WhatsAppScenario.WriteMessage(appium, number, richTextBoxMessage.Text);
                        await Task.Delay(r.Next(10000, 20000));
                        break;
                    }
                    catch
                    {
                        iterations++;
                        await Task.Delay(5000);
                    }
                }
                
            }
            
            
            /*Random r = new Random();
            int count = 0;
            var startTime = DateTime.Now.ToLocalTime();
            while (true)
            {
                int number = r.Next(7999999) + 2000000;

                string response = await Task.Run(() => ms.sendMessageAsync("3712" + number.ToString(), r.Next().ToString()));
                
                listBoxServerAnswers.Items.Add(response);
                if (response.Contains("not equals authenticated."))
                {
                    return;
                }
                count++;
                string timediff = (DateTime.Now.ToLocalTime() - startTime).ToString();
                labelServerAnswer.Text = "Server answers: (Count:" + count.ToString() + " Timediff: " + timediff + ")";
            }*/
        }

        [Obsolete]
        private async void buttonAppiumRun_Click(object sender, EventArgs e)
        {
            try
            {
                Invoke(new Action(() => appium = new AppiumDevice(Apps.WhatsApp, Apps.WhatsAppActivity_Home, new Device(comboBoxAppium.Text))));
                checkActivity.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonAppiumClick_Click(object sender, EventArgs e)
        {
            try
            {
                appium.GetElementByID(comboBoxWhatsAppIds.Text).Tap(1, 5);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void buttonAppiumSendText_Click(object sender, EventArgs e)
        {
            try
            {
                appium.GetElementByID(comboBoxWhatsAppIds.Text).SendKeys(textBoxAppiumSendKeys.Text);
                // appium.SendText(textBoxAppiumSendKeys.Text, textBoxAppiumElementId.Text);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        public async void AutomatizationCancellable(CancellationTokenSource cancellationTokenSource)
        {
            checkActivity.Enabled = true;
            bool banned = false;
            // string command = "cd " + textBoxAppiumSendKeys.Text;
            //CommandExecutor.ExecuteCommandAsync(command);
            int iterations = 0;
            string command = "emulator @" + comboBoxAppium.Text;
            if (textBoxProxy.Text != "")
            {
                command += " -http-proxy " + textBoxProxy.Text;
                System.IO.File.AppendAllText("usedproxy.txt", textBoxProxy.Text + "\n");

            }
            command += " -wipe-data -no-snapshot-load";

            CommandExecutor.ExecuteCommandAsync(command);
            await Task.Delay(25000);


            iterations = 0;
            while (iterations < 60)
            {
                try
                {
                    appium = new AppiumDevice(Apps.Contacts,
                        Apps.ContatsActivity_Main,
                        new Device(comboBoxAppium.Text));
                    SendVCard();
                    break;
                }
                catch
                {
                    iterations++;
                    await Task.Delay(5000);
                }
            }
            iterations = 0;
            while (iterations < 60)
            {
                try
                {
                    ContactsScenario.ImportContacts(appium);
                    break;
                }
                catch
                {
                    iterations++;
                    await Task.Delay(5000);
                }
            }

            await Task.Delay(2000);


            string number = "";
            iterations = 0;
            while (iterations < 60)
            {
                try
                {
                    number = await smsActivate.GetNumber();
                    number = number.Remove(0, 1);
                    break;
                }
                catch
                {
                    iterations++;
                    await Task.Delay(5000);
                }
            }
            do
            {
                
                Random random = new Random();
                iterations = 0;
                while (iterations < 60)
                {
                    try
                    {
                        appium = new AppiumDevice(Apps.WhatsApp,
                              Apps.WhatsAppActivity_Eula,
                              new Device(comboBoxAppium.Text));
                       // await Task.Delay(random.Next(4000, 6000));
                        banned = await WhatsAppScenario.Registration(appium, number, "7");
                        if (banned)
                        {
                            appium.CloseApp();
                            smsActivate.setStatus("7" + number, "10");
                            smsActivate.setStatus("7" + number, "-1");
                            iterations = 0;
                            while (iterations < 60)
                            {
                                try
                                {
                                    number = await smsActivate.GetNumber();
                                    number = number.Remove(0, 1);
                                    break;
                                }
                                catch
                                {
                                    iterations++;
                                    await Task.Delay(5000);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        iterations++;
                        await Task.Delay(5000);
                    }
                }
            } while (!banned);


            smsActivate.PhoneReady("7" + number);
            iterations = 0;
            while (iterations < 1200)
            {
                try
                {
                    string code = await smsActivate.GetCode("7" + number);
                    if (code != "STATUS_WAIT_CODE")
                    {

                        WhatsAppScenario.VerifyCode(appium, code);

                    }
                    else
                    {
                        iterations++;
                        await Task.Delay(1000);
                    }

                }
                catch (Exception ex)
                {
                    iterations++;

                    await Task.Delay(1000);
                }
            }

            smsActivate.setStatus("7" + number, "6");

            iterations = 0;
            while (iterations < 60)
            {
                try
                {
                    WhatsAppScenario.WriteName(appium, textBoxName.Text);
                }
                catch
                {
                    iterations++;
                    await Task.Delay(5000);
                }
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(Properties.Settings.Default.PathToDirectory + "/success.wav");
            player.Play();
            // devices.Add(new Device(comboBoxAppium.Text));
            // new DeviceForm(comboBoxAppium.Text, textBoxProxy.Text).Show();
        }
        private async void buttonEmulatorRun_Click(object sender, EventArgs e)
        {
            Invoke(new Action(async () =>
            {
                automatizationTokenSource = new CancellationTokenSource();
                
                try
                {
                    AutomatizationCancellable(automatizationTokenSource);
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Canceled");
                }
                
                
            }));

        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                appium.GetElementByID(comboBoxWhatsAppIds.Text).Clear();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonGetProxy_Click(object sender, EventArgs e)
        {
            //string response = await ApiRequest.GetRequestAsync("https://www.proxy-list.download/api/v1/get?type=http&country=RU");
            string response = await ApiRequest.GetRequestAsync("https://my.virty.io/proxy_list/proxies.php?hash=a0ab9195a8296439d166db4dbaac1a68&type=http&format=format3");
            string[] adresses = response.Split('\n');
          
            Random rand = new Random();
            int index = rand.Next(0, adresses.Length);
            textBoxProxy.Text = adresses[index].Trim();

        }

        private void buttonRegistration_Click(object sender, EventArgs e)
        {
            Invoke(new Action(() => WhatsAppScenario.Registration(appium, textBoxPhone.Text, textBoxCC.Text)));
        }

        private void buttonVerifyCode_Click(object sender, EventArgs e)
        {
            Task.Run(() => WhatsAppScenario.VerifyCode(appium, textBoxCode.Text));
        }

        private async void buttonGetPhone_Click(object sender, EventArgs e)
        {
            try
            {
                string number = await smsActivate.GetNumber();
                textBoxCC.Text = "7";
                textBoxPhone.Text = number.Remove(0, 1);
                timerGetCode.Enabled = true;
                try
                {
                    smsActivate.PhoneReady(number);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "SMS ACTIVATE");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "SMS ACTIVATE");
                MessageBox.Show(ex.StackTrace, "SMS ACTIVATE");
            }
            
        }
        
        private async void buttonGetCode_Click(object sender, EventArgs e)
        {

            
        }
        private void checkActivity_Tick(object sender, EventArgs e)
        {
            try
            {
                string activity = appium.GetCurrentActivity();
                labelActivity.Text = activity;
                switch (activity)
                {
                    case "":

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                labelActivity.Text = ex.Message;
            }
        }

        private async void buttonGenerateVcard_Click(object sender, EventArgs e)
        {
            try
            {
                string vCardText = await Task.Run(() => VCardGenerator());
                await Task.Run(() =>
                {
                    System.IO.File.WriteAllText(Properties.Settings.Default.PathToDirectory + "/vcard.vcf", vCardText);
                });
                MessageBox.Show("VCard ready!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
               
            }
            
        }
    
        private string VCardGenerator()
        {
            List<string> numbers = new List<string>();
            var numberBases = ms.GetNumberBases();
            foreach (var numberBase in numberBases.Values)
            {
                numbers = numbers.Concat(numberBase.Numbers).ToList();
            }
            string vcards = "";
            for (int i = 0; i < numbers.Count; i++)
            {

                vcards += "BEGIN:VCARD\nVERSION:2.1\nN:;+" + numbers[i] + ";;;\nFN:+" + numbers[i] + "\nTEL;CELL:+" + numbers[i] + "\nEND:VCARD\n";
            }
            return vcards;
        }

        private async void buttonSendVcard_Click(object sender, EventArgs e)
        {

            Invoke(new Action(() =>
            {
                SendVCard();
            }));

        }
        private void SendVCard()
        {
            string command = "cd " + textBoxSDKPath.Text + "/platform-tools";
            CommandExecutor.ExecuteCommandAsync(command);
            command = " push " + Properties.Settings.Default.PathToDirectory + "/vcard.vcf /sdcard";
            CommandExecutor.AdbExecutor(command);
        }

        private async void timerGetCode_Tick(object sender, EventArgs e)
        {
            try
            {
                string code = await smsActivate.GetCode(textBoxCC.Text + textBoxPhone.Text);
                if (code != "STATUS_WAIT_CODE")
                {
                    textBoxCode.Text = code;
                    timerGetCode.Enabled = false;
                    WhatsAppScenario.VerifyCode(appium,code);
                }
                
            }catch (Exception ex)
            {
                timerGetCode.Enabled = false;
                MessageBox.Show(ex.Message, "SMS ACTIVATE");
            }
        }

        [Obsolete]
        private void buttonAppiumContacts_Click(object sender, EventArgs e)
        {
            
            Invoke(
                new Action(() => 
                    appium = new AppiumDevice(Apps.Contacts, Apps.ContatsActivity_Main, new Device(comboBoxAppium.Text)
                    )
                )
            );
            ContactsScenario.ImportContacts(appium);
        }

        private void buttonEmulator_Click(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
            {
                checkActivity.Enabled = true;
                // string command = "cd " + textBoxAppiumSendKeys.Text;
                //CommandExecutor.ExecuteCommandAsync(command);
                int iterations = 0;
                string command = "emulator @" + comboBoxAppium.Text;
                if (textBoxProxy.Text != "")
                {
                    command += " -http-proxy " + textBoxProxy.Text;
                    System.IO.File.AppendAllText("usedproxy.txt", textBoxProxy.Text + '\n');

                }
                command += " -wipe-data -no-snapshot-load";

                CommandExecutor.ExecuteCommandAsync(command);

            }));
        }

        private void buttonGetActivity_Click(object sender, EventArgs e)
        {
            labelActivity.Text = CommandExecutor.ExecuteCommandSync("adb shell dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'");
        }

        private void buttonStopAppium_Click(object sender, EventArgs e)
        {
            automatizationTokenSource.Cancel();
            appium = null;
        }
    }
}

