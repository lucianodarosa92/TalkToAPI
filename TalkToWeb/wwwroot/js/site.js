function TesteCors() {
    var tokenJWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Imx1Y2lhbm8yQGdtYWlsLmNvbSIsInN1YiI6IjY0YzZhMTA4LTQ5NWQtNGE5Ny04OWY5LWNkMDBhNTM3ZDJhMyIsImV4cCI6MTY4MTkzMDUzMX0.GcnSGuMvVjt7PnDoJ8rTX1_dTMWZ6Vny8NzggZNYtZQ";
    var servico = "https://localhost:44398/api/usuario/ObterTodos";
    $("#resultado").html("---Solicitando---");
    $.ajax({
        url: servico,
        method: "GET",
        crossDomain: true,
        headers: { "Accept": "application/json" },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + tokenJWT);
        },
        success: function (data, status, xhr) {
            $("#resultado").html(data);
            console.info(data);
        }
    });
}