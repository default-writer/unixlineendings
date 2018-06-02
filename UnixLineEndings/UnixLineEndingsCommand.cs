using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Build.VSIXProject
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class UnixLineEndingsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f8687f58-2ac3-4c7e-8c4e-a3320b8648e9");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixLineEndingsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        UnixLineEndingsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static UnixLineEndingsCommand Instance
        {
            get;
            private set;
        }

        public static DTE2 Dte
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in UnixLineEndingsCommand's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            Dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new UnixLineEndingsCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var outputWindow = Dte.ToolWindows.OutputWindow;
            OutputWindowPane outputWindowPane = null;
            string windowPaneName = "Unix Conversion Tool Window";
            foreach (OutputWindowPane windowPane in outputWindow.OutputWindowPanes)
            {
                if (windowPane.Name == windowPaneName)
                {
                    outputWindowPane = windowPane;
                    break;
                }
            }
            // Make sure all docs are searched before displaying results.
            outputWindowPane = outputWindowPane ?? outputWindow.OutputWindowPanes.Add(windowPaneName);
            outputWindowPane.Activate();

            outputWindowPane.OutputString(string.Format("{0}{1}", "Replacing all occurrences of 'CRLF' with 'LF'", Environment.NewLine));
            dynamic document = Dte.ActiveDocument;
            var openContext = Dte.UndoContext.IsOpen;
            try
            {
                if (!openContext)
                {
                    //' Open the UndoContext object to track changes.
                    Dte.UndoContext.Open(string.Format("{0} - {1}{2}", "Convert to Unix", document.Name, Environment.NewLine), false);
                }
                document.ReplaceText("\r\n", "\n");
                outputWindowPane.OutputString(string.Format("{0}: {1}{2}", "Replaced all occurrences of 'CRLF' with 'LF'", document.FullName, Environment.NewLine));
            }
            catch (Exception ex)
            {
                outputWindowPane.OutputString(string.Format("{0}{1}", "Error: " + ex.Message, Environment.NewLine));
            }
            finally
            {
                if (openContext)
                {
                    // Close the UndoContext object to commit the changes.
                    Dte.UndoContext.Close();
                }
                document.Save();
            }

#if VS_MESSAGEBOX
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "UnixLineEndingsCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
#endif
        }
    }
}