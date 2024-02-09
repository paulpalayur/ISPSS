const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
document.addEventListener('DOMContentLoaded', function () {
    var clipboard = new ClipboardJS('.bi-clipboard');
    clipboard.on('success', function (e) {

        $('.bi-clipboard').css('display', 'none');
        $('.bi-clipboard-check-fill').css('display', 'block');
        window.setTimeout(toggleClipboardIcon, 1000);
        e.clearSelection();
    });
});

function toggleClipboardIcon() {
    $('.bi-clipboard-check-fill').css('display', 'none');
    $('.bi-clipboard').css('display', 'block');
}