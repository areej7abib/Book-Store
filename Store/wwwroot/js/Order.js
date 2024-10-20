var datatable;
$(document).ready(function () {
    LoadDatatable();
});

function LoadDatatable() {
    datatable = $('#tbldata').DataTable({
        "ajax": {
            url: '/admin/order/getall',
            type: 'GET',
            dataSrc: 'data',
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('Error loading data:', textStatus, errorThrown);
                alert('Failed to load data from the server.');
            }
        },
        "columns": [
            { data: 'id', className: "text-center", "width": "25%" },
            { data: 'name', className: "text-center", "width": "15%" },
            { data: 'phoneNumber', className: "text-right", "width": "10%" },
            { data: 'applicationUser.email', className: "text-left", "width": "15%" },
            { data: 'orderStatus', className: "text-left", "width": "10%" },
            { data: 'orderTotal', className: "text-left", "width": "10%" }, {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                "width": "25%"
            }
        ],
        "processing": true,  
        "serverSide": false  
    });
}