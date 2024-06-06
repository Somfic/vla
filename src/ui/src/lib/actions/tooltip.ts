import Tooltip from '$lib/components/Tooltip.svelte';

export function tooltip(
	element: HTMLElement,
	params: { text: string; position: 'left' | 'top' | 'right' | 'bottom' }
) {
	element.style.position = 'relative';
	// eslint-disable-next-line no-undef
	let timeout: NodeJS.Timeout;

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
		clearTimeout(timeout);
		tooltipElement.$set({ show: false });
	}

	function mouseLeave() {
		clearTimeout(timeout);
		tooltipElement.$set({ show: false });
	}

	function mouseMove() {
		clearTimeout(timeout);
		tooltipElement.$set({ show: false });
		timeout = setTimeout(() => {
			tooltipElement.$set({ show: true });
		}, 500);
	}

	element.addEventListener('mouseover', mouseOver);
	element.addEventListener('mouseleave', mouseLeave);
	element.addEventListener('mousemove', mouseMove);

	return {
		destroy() {
			element.removeEventListener('mouseover', mouseOver);
			element.removeEventListener('mouseleave', mouseLeave);
			element.removeEventListener('mousemove', mouseMove);
			tooltipElement?.$destroy();
		}
	};
}
