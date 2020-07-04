var validationGroups = document.getElementsByName('validation-group');
validationGroups.forEach(group => {
	var labelChild = group.querySelector('.form-group-label');
	var inputChild = group.querySelector('.form-group-input');
	var validateSpan = group.querySelector('.text-danger');

	if (validateSpan.innerHTML != "") {
		labelChild.style.color = 'red';
		inputChild.style.borderColor = 'red';
	}
});