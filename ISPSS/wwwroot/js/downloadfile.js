$(document).ready(function () {
    $(".DownloadFile").click(function () {        
        var content = $(".requirements>pre").html();
        
        $.ajax({
            type: "POST",
            url: "/Home/DownloadFile",
            data: { content: content },
            success: function (response) {                
                var blob = new Blob([response], { type: 'text/plain' });

                // Create a link and trigger a click to initiate download
                var link = document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = "ISP_Networks.txt";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            },
            error: function (error) {
                console.error("Error:", error);
            }
        });
    });
});
