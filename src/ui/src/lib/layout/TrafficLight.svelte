<script lang="ts"></script>

<div class="traffic-light">
	<div class="button red"></div>
	<div class="button yellow"></div>
	<div class="button green"></div>
</div>

<style lang="scss">
	$close-red: #ff6159;
	$close-red-active: #bf4942;
	$close-red-icon: #4d0000;
	$close-red-icon-active: #190000;

	$minimize-yellow: #ffbd2e;
	$minimize-yellow-active: #bf8e22;
	$minimize-yellow-icon: #995700;
	$minimize-yellow-icon-active: #592800;

	$maximize-green: #28c941;
	$maximize-green-active: #1d9730;
	$maximize-green-icon: #006500;
	$maximize-green-icon-active: #003200;

	$disabled-gray: #ddd;

	.traffic-light {
		display: flex;
		flex-direction: row;
		align-items: center;
		justify-content: space-between;
		width: 50px;

		div {
			width: 10px;
			height: 10px;
			border-radius: 50%;

			&.red {
				background-color: red;
			}

			&.yellow {
				background-color: yellow;
			}

			&.green {
				background-color: green;
			}
		}
	}
</style>
