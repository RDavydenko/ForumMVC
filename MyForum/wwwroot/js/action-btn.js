var btns = document.getElementsByClassName('action-btn');
for (let i = 0; i < btns.length; i++) {
	btns[i].addEventListener('click', function (e) {
		var actionsMenu = btns[i].querySelector('.action-btn__actions');
		actionsMenu.style.display = 'block';
		window.addEventListener('click', (e) => {
			if (!e.target.matches('.action-btn__actions') && !e.target.matches('.action-btn')) {
				actionsMenu.style.display = 'none';
			}
		});
	});
}