function createPopup(type, text) {
    $("#popup-block").append('<div style="border:1px solid darkgray;" class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' + text + '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
}