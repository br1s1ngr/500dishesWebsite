/**
 * Created by kola on 7/24/2016.
 */

$(function () {
    var signupForm = $('.js-sign-up-form');
    var loginForm = $('.js-login-form');
    var logIn = $('.js-login');
    var signUp = $('.js-signup');

    logIn.click(function () {
        signupForm.css('display', 'none');
        loginForm.css('display', 'block');
    });

    signUp.click(function () {
        signupForm.css('display', 'block');
        loginForm.css('display', 'none');
    });

    $('.form-control').blur(function () {
        if ($(this).val())
            $(this).addClass('used');
        else $(this).removeClass('used');
    });
});