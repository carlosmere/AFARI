$(document).on('click', '[data-type="modal-link"]', function (e) {
    e.preventDefault();

    // Url de vista a cargar
    var sourceUrl = $(this).attr('data-source-url');

    var $modalLoading = $('#default-modal-loading');
    var $modal = $('<div class="modal fade"><div class="modal-dialog"><div class="modal-content"></div></div></div>');
    var $modalContent = $modal.find('.modal-content');

    $modalContent.html($modalLoading.html());

    var $container = $('#default-modal-container');
    $container.append($modal);

    var bootModal = $modal.modal('show');

    //.on('shown.bs.modal', function () {
    $modalContent.load(sourceUrl, function (response, status, xhr) {
        if (status == "error") {
            //alert("error");
            $modalContent.html($('#default-modal-loading-error').html());
        }
        else {
            $.validator.unobtrusive.parse($modalContent);
        }
    });
    // });

    bootModal.on('hidden.bs.modal', function () {
        $(this).remove();
    });
});

//$('.datepicker').datepicker({ format: 'dd/mm/yyyy', weekStart: 1 })
function RebindJquery($element) {
    $element.find('.datepicker').datepicker({ format: "dd/MM/yyyy", weekStart: 1, autoclose: true });
    $element.find('.select2').select2();
}

RebindJquery($(document));

function KeepAlive() {
    http_request = new XMLHttpRequest();
    http_request.open('GET', "/Home/KeepAlive");
    http_request.send(null);
};

setInterval(KeepAlive, 300000);