shader_type canvas_item;

uniform sampler2D viewport : hint_screen_texture, filter_nearest;

vec4 schwabe(vec2 uv, float thresh, float y) {
	float dist = abs(uv.y - y);
	return vec4(0, 0.2, 0, 0) * smoothstep(thresh, 0.0, dist);
}

void fragment() {
	COLOR = texture(viewport, SCREEN_UV);
	COLOR += vec4(0, 0.2, 0, 0);
	COLOR += schwabe(UV, 0.1, 0.5 + 0.01 * sin(10.0 * UV.x + TIME));
	COLOR += schwabe(UV, 0.1, 0.2 + 0.1 * sin(UV.x + TIME));
	COLOR += schwabe(UV, 0.1, 0.8 + 0.05 * sin(5.0 * UV.x - TIME));
}
