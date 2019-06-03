$(document).ready(function () {
    dropdownCourse();
});

function dropdownCourse() {
    $(document).on('click', '.course-title', function () {
        $(this).next('.preorder-group').slideToggle('slow');
    });
}

