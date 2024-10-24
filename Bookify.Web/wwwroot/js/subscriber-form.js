$(document).ready(function () {
    $('#governorateSelect').change(function () {
        var governorateId = $(this).val();
        $('#citySelect').empty().append('<option value="">Select an area...</option>'); // Clear current areas

        if (governorateId) {
            $.getJSON('/Subscribers/GetCitiesByGovernorate', { governorateId: governorateId }, function (data) {
                $.each(data, function (index, item) {
                    $('#citySelect').append($('<option>').val(item.value).text(item.text));
                });
            });
        }
    });



});

