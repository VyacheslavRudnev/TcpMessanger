using Networking;
using System.Text;

namespace TcpMessanger
{
    public partial class Form1 : Form
    {
        private TcpManager _tcpManager;

        public Form1()
        {
            InitializeComponent();
            _tcpManager = new TcpManager();
            _tcpManager.Received += _tcpManager_Received;
        }

        private void _tcpManager_Received(Request request)
        {
            string message = "";
            switch (request.Path)
            {
                case "message":
                    message = Encoding.UTF8.GetString(request.Data);
                    break;
                case "error":
                    message = $"Error: {Encoding.UTF8.GetString(request.Data)}";
                    break;
            }

            this.Invoke(() => listBox.Items.Add(message));
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(addressTb.Text) || string.IsNullOrWhiteSpace(portTb.Text) || string.IsNullOrWhiteSpace(nameTb.Text))
            {
                MessageBox.Show("Заповніть всі поля.");
                return;
            }

            try
            {
                _tcpManager.Connect(addressTb.Text.Trim(), int.Parse(portTb.Text.Trim()));
                Request request = new Request()
                {
                    Path = "login",
                    Data = Encoding.UTF8.GetBytes(nameTb.Text.Trim())
                };
                _tcpManager.Send(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(messageTb.Text))
            {
                MessageBox.Show("Введіть ваше повідомлення.");
                return;
            }

            Request request = new Request()
            {
                Path = "message",
                Data = Encoding.UTF8.GetBytes(messageTb.Text.Trim())
            };
            _tcpManager.Send(request);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                Request request = new Request()
                {
                    Path = "file",
                    Data = ms.ToArray()
                };
                _tcpManager.Send(request);
            }
        }
    }
}
