shader_type canvas_item;

uniform sampler2D viewport : hint_screen_texture, filter_nearest;

vec3 schwabe(vec2 uv, float thresh, float y) {
	float dist = abs(uv.y - y);
	return vec3(0, 0.2, 0) * smoothstep(thresh, 0.0, dist);
}

void fragment() {
	COLOR = texture(viewport, SCREEN_UV);
	COLOR.rgb += vec3(0, 0.2, 0);
	COLOR.rgb += schwabe(UV, 0.5, 0.5 + 0.05 * sin(10.0 * UV.x + TIME));
	COLOR.rgb += schwabe(UV, 0.1, 0.2 + 0.1 * sin(UV.x + TIME));
	COLOR.rgb += schwabe(UV, 0.1, 0.8 + 0.05 * sin(5.0 * UV.x - TIME));
}
