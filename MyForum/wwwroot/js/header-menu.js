var menuDiv = document.getElementById('account-blog-menu');
var submenu = document.getElementById('account-blog-menu__submenu');
menuDiv.addEventListener('click', (e) => {
    if (submenu.style.display === 'none') {
        menuDiv.style.backgroundColor = '#2b4d8d';
        submenu.style.display = 'block';
    }
    else {
        menuDiv.style.backgroundColor = '';
        submenu.style.display = 'none';
    }
});
$(document).click(e => {
    var elems = $("#account-blog-menu *");
    console.log(elems);

    var isNotClicked = true;
    for (let i = 0; i < elems.length; i++) {
        if (e.target == elems[i]) {
            isNotClicked = false;
            break;
        }
    }
    if (isNotClicked) {
        menuDiv.style.backgroundColor = '';
        submenu.style.display = 'none';
    }
});