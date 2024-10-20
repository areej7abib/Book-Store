var datatable;
$(document).ready(function () {
    LoadDatatable();
});

function LoadDatatable() {
    datatable = $('#tbldata').DataTable({
        "ajax": {
            url: '/admin/product/getall',
            type: 'GET',
            dataSrc: 'data',
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('Error loading data:', textStatus, errorThrown);
                alert('Failed to load data from the server.');
            }
        },
        "columns": [
            { data: 'title', className: "text-center", "width": "25%" },
            { data: 'isbn', className: "text-center", "width": "15%" },
            { data: 'listPrice', className: "text-right", "width": "10%" },
            { data: 'author', className: "text-left", "width": "15%" },
            { data: 'category.name', className: "text-left", "width": "10%" }, {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>               
                     <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ],
        "processing": true,  
        "serverSide": false  
    });
}
function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    datatable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    })
}