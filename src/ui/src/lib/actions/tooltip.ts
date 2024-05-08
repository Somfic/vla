export function tooltip(
	element: HTMLElement,
	params: { text: string; position: 'left' | 'top' | 'right' | 'bottom' }
) {
	element.classList.add('tooltip');
}
