namespace MouseTracker
{
	public partial class SurrogateForm : Form
	{
		private readonly Action<Message> messageProcess;

		public SurrogateForm(Action<Message> messageProcess)
		{
			InitializeComponent();
			this.messageProcess=messageProcess;
		}

		protected override void WndProc(ref Message m)
		{
			messageProcess.Invoke(m);
			base.WndProc(ref m);
		}
	}
}
