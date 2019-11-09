using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FileCryption
{
    public partial class MainForm : Form
    {
        public string SettingFilePath =
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\"
    + Path.GetFileNameWithoutExtension(Application.ExecutablePath)
    + ".dat";
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                comboBoxMode.SelectedIndex = 0;
                comboBoxAlg.SelectedIndex = 0;
                if (File.Exists(SettingFilePath))
                {
                    FileStream fileStream = new FileStream(SettingFilePath, FileMode.Open, FileAccess.Read);
                    BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.ASCII);
                    this.Top = binaryReader.ReadInt32();
                    this.Left = binaryReader.ReadInt32();
                    comboBoxMode.SelectedIndex = binaryReader.ReadInt32();
                    binaryReader.Close();
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                FileStream fileStream = new FileStream(SettingFilePath, FileMode.Create, FileAccess.Write);
                BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.ASCII);
                binaryWriter.Write(this.Top);
                binaryWriter.Write(this.Left);
                binaryWriter.Write(comboBoxMode.SelectedIndex);
                binaryWriter.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK);
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.RestoreDirectory = true;
                //ofd.Filter = "所有文件 (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBoxPath.Text = ofd.FileName;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                string FilePath = textBoxPath.Text.Trim();
                if (File.Exists(FilePath))
                {
                    string Key = textBoxKey.Text;
                    if (Key.Length > 32) throw new Exception("密钥长度不能超过32位。");
                    int index = comboBoxMode.SelectedIndex;
                    switch (index)
                    {
                        case 0:
                            Encrypt(FilePath, Key);
                            MessageBox.Show("加密成功。", this.Text);
                            break;
                        case 1:
                            Decrypt(FilePath, Key);
                            MessageBox.Show("解密成功。", this.Text);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("文件不存在。", this.Text, MessageBoxButtons.OK);
                }
            }
            catch(CryptographicException)
            {
                if (comboBoxMode.SelectedIndex == 0)
                {
                    MessageBox.Show("加密失败", this.Text);
                }
                else
                {
                    MessageBox.Show("解密失败", this.Text);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK);
            }
        }

        public void Encrypt(string path,string key)
        {
            FileStream fsread = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fsread.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)fsread.ReadByte();
            }
            fsread.Close();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string sKey = key.PadRight(aes.Key.Length);
            byte[] inputByteArray = data;
            aes.Key = Encoding.Default.GetBytes(sKey);
            aes.IV = Encoding.Default.GetBytes(sKey.Substring(0, aes.IV.Length));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] retB = ms.ToArray();
            FileStream fswrite = new FileStream(path, FileMode.Create, FileAccess.Write);
            for(int i = 0; i < retB.Length; i++)
            {
                fswrite.WriteByte(retB[i]);
            }
            fswrite.Close();
        }
        public void Decrypt(string path, string key)
        {
            FileStream fsread = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fsread.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)fsread.ReadByte();
            }
            fsread.Close();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string sKey = key.PadRight(aes.Key.Length);
            byte[] inputByteArray = data;
            aes.Key = Encoding.Default.GetBytes(sKey);
            aes.IV = Encoding.Default.GetBytes(sKey.Substring(0, aes.IV.Length));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] retB = ms.ToArray();
            FileStream fswrite = new FileStream(path, FileMode.Create, FileAccess.Write);
            for (int i = 0; i < retB.Length; i++)
            {
                fswrite.WriteByte(retB[i]);
            }
            fswrite.Close();
        }
    }
}
