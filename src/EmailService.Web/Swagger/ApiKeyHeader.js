(function () {
    $(function () {
        $('#input_apiKey').off();
        $('#input_apiKey').on('change', function () {
            var key = this.value;
            if (key && key.trim() !== '') {
                swaggerUi.api.clientAuthorizations.add("key", new SwaggerClient.ApiKeyAuthorization("Authorization", key, "header"));
            }
        });
    });
})();