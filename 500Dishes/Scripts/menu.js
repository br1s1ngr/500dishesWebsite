/**
 * Created by kola on 7/26/2016.
 */
var support = { animations : Modernizr.cssanimations },
    animEndEventNames = { 'WebkitAnimation' : 'webkitAnimationEnd', 'OAnimation' : 'oAnimationEnd', 'msAnimation' : 'MSAnimationEnd', 'animation' : 'animationend' },
    animEndEventName = animEndEventNames[ Modernizr.prefixed( 'animation' ) ],
    onEndAnimation = function( el, callback ) {
        var onEndCallbackFn = function( ev ) {
            if( support.animations ) {
                if( ev.target != this ) return;
                this.removeEventListener( animEndEventName, onEndCallbackFn );
            }
            if( callback && typeof callback === 'function' ) { callback.call(); }
        };
        if( support.animations ) {
            el.addEventListener( animEndEventName, onEndCallbackFn );
        }
        else {
            onEndCallbackFn();
        }
    };

var elements = document.querySelectorAll(".add-to-menu");
for (var i = 0; i < elements.length; i++) {
    elements[i].addEventListener("click", addToCart);
}

var cart = document.querySelector('.cart');
var cartItems = cart.querySelector('.cart__count');

function addToCart() {
    classie.add(cart, 'cart--animate');
    setTimeout(function() {cartItems.innerHTML = Number(cartItems.innerHTML) + 1;}, 200);
    onEndAnimation(cartItems, function() {
        classie.remove(cart, 'cart--animate');
    });
}