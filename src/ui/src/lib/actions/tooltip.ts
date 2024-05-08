import Tooltip from '$lib/components/Tooltip.svelte';

export function tooltip(
	element: HTMLElement,
	params: { text: string; position: 'left' | 'top' | 'right' | 'bottom' }
) {
	element.style.position = 'relative';

	const tooltipElement = new Tooltip({
		target: element,
		props: {
			text: params.text,
			position: params.position,
			show: false,
			parentSize: {
				width: element.offsetWidth,
				height: element.offsetHeight
			}
		}
	});

	function mouseOver() {
		tooltipElement.$set({ show: true });
	}

	function mouseLeave() {
		tooltipElement.$set({ show: false });
	}

	element.addEventListener('mouseover', mouseOver);
	element.addEventListener('mouseleave', mouseLeave);

	return {
		destroy() {
			element.removeEventListener('mouseover', mouseOver);
			element.removeEventListener('mouseleave', mouseLeave);
			tooltipElement?.$destroy();
		}
	};
}
