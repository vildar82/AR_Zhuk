using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_AreaZhuk.Model;
using AR_AreaZhuk.PIK1TableAdapters;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            TextWriterTraceListener writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Перехват необработанных исключений
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Error(ex);                
            }             
        }

        private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            Error((Exception)e.ExceptionObject);
        }

        private static void Application_ThreadException (object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Error(e.Exception);
        }

        private static void Error (Exception ex)
        {
            // Если работает прогрессбар - остановка
            MainForm.ProgressThreadStop();       
            // отправка сообщения об ошибке на почту
            SendMail(ex);
            // показ ошибки пользователю
            MessageBox.Show("Ошибка в программе.\n\r" + ex.Message, "Жуки", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }        

        public static void SendMail (Exception ex)
        {
            try
            {
                MailMessage mail = new MailMessage(Environment.UserName + "@pik.ru", "vildar82@gmail.com, inkinleo@gmail.com");
                mail.Subject = "Жуки. Ошибка у " + Environment.UserName;
                mail.Body = ex.ToString();
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = "ex20pik.picompany.ru";
                client.Send(mail);
            }
            catch { }
        }        
    }
}
