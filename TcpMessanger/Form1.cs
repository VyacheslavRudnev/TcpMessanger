using Networking;
using System.Text;

namespace TcpMessanger;

public partial class Form1 : Form
{
    private TcpManager _tcpManager;

    public Form1()
    {
        InitializeComponent();
        tabControl1.SelectedIndex = 1;
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
                this.Invoke(() => MessageBox.Show(message));
                _tcpManager.Disconnect();
                this.Invoke(() => this.Close());
                return;
            case "logout":
                _tcpManager.Disconnect();
                this.Invoke(() => this.Close());
                return;
        }

        this.Invoke(() => listBox.Items.Add(message));
    }

    private void connectBtn_Click(object sender, EventArgs e)
    {
        string username = nameTb.Text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            MessageBox.Show("Будь ласка, введіть ім'я користувача.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            tabControl1.Enabled = true;
            tabControl1.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(messageTb.Text.Trim()))
        {
            MessageBox.Show("Будь ласка, введіть текст повідомлення.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            Request request = new Request()
            {
                Path = "message",
                UserName = nameTb.Text.Trim(),
                Data = Encoding.UTF8.GetBytes(messageTb.Text.Trim())
            };
            _tcpManager.Send(request);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не вдалося відправити повідомлення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

    }

    private void button2_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == DialogResult.OK)
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

    private void textBox_TextChanged(object sender, EventArgs e)
    {
        connectBtn.Enabled = !(string.IsNullOrEmpty(nameTb.Text.Trim()));
    }

    private void button3_Click(object sender, EventArgs e)
    {
        try
        {
            Request request = new Request()
            {
                Path = "logout",
                UserName = nameTb.Text.Trim(),
                Data = Encoding.UTF8.GetBytes(nameTb.Text.Trim())
            };
            _tcpManager.Send(request);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не вдалося завершити сеанс: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}


