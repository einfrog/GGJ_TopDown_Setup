shader_type canvas_item;

uniform sampler2D viewport : hint_screen_texture, filter_nearest;
uniform vec3 gas_color : source_color;
uniform vec3 vapor_color : source_color;

vec3 vapor(vec2 uv, float thresh, float y) {
	float dist = abs(uv.y - y);
	return vapor_color * smoothstep(thresh, 0.0, dist);
}

void fragment() {
	vec3 gas = gas_color;
	gas += vapor(UV, 0.5, 0.5 + 0.05 * sin(10.0 * UV.x + TIME));
	gas += vapor(UV, 0.1, 0.2 + 0.1 * sin(UV.x + TIME));
	gas += vapor(UV, 0.1, 0.8 + 0.05 * sin(5.0 * UV.x - TIME));
	COLOR = vec4(gas, 0) + texture(viewport, SCREEN_UV);
}
