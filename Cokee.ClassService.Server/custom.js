window.onload = function() {
    const authHeader = document.querySelector("input[type='text'][name='Authorization']");

    if (authHeader) {
        authHeader.placeholder = "Bearer <Token>";
    }
};