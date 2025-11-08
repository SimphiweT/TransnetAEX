namespace TransnetAEX.Application.Helpers
{
    public static class EmailTemplateHelper
    {
        public static string BuildTemplate(string logoUrl, string title, string body)
        {
            return $@"
                <div style='font-family:Segoe UI,Arial,sans-serif;background-color:#f8f9fa;padding:20px;'>
                    <div style='max-width:600px;margin:auto;background:#fff;border-radius:10px;
                                box-shadow:0 2px 8px rgba(0,0,0,0.05);overflow:hidden;'>
                        <div style='background:#d71920;color:white;padding:15px 20px;
                                    display:flex;align-items:center;gap:10px;'>
                            <img src='{logoUrl}' alt='Transnet Logo' style='height:45px;border-radius:6px;' />
                            <h2 style='margin:0;font-weight:500;'>Transnet AEX System</h2>
                        </div>
                        <div style='padding:25px;color:#333;'>
                            <h3 style='color:#d71920;margin-bottom:15px;'>{title}</h3>
                            {body}
                        </div>
                        <div style='padding:10px 25px;font-size:13px;color:#666;border-top:1px solid #eee;'>
                            This is an automated message from the Transnet Exit Management System.
                        </div>
                    </div>
                </div>";
        }
    }
}
