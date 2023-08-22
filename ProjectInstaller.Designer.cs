namespace AppCloud_Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.AppCloudServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.AppCloudService = new System.ServiceProcess.ServiceInstaller();
            // 
            // AppCloudServiceProcessInstaller1
            // 
            this.AppCloudServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.AppCloudServiceProcessInstaller1.Password = null;
            this.AppCloudServiceProcessInstaller1.Username = null;
            // 
            // AppCloudService
            // 
            this.AppCloudService.Description = "Serviço de autenticação App Cloud TSMIT";
            this.AppCloudService.DisplayName = "App Cloud Tsmit Service";
            this.AppCloudService.ServiceName = "AppCloud-Service";
            this.AppCloudService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.AppCloudServiceProcessInstaller1,
            this.AppCloudService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller AppCloudServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller AppCloudService;
    }
}