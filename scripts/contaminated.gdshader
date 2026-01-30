shader_type canvas_item;

uniform sampler2D viewport : hint_screen_texture, filter_nearest;

void fragment() {
	COLOR = texture(viewport, SCREEN_UV);
	COLOR += vec4(0, 0.2, 0, 0);
	float y1 = 0.5 + 0.01 * sin(10.0 * UV.x + TIME);
	float y2 = 0.2 + 0.1 * sin(UV.x + TIME);
	float y3 = 0.8 + 0.05 * sin(5.0 * UV.x - TIME);
	
	if (abs(UV.y - y1) < 0.01) {
		COLOR += vec4(0, 0.2, 0, 0);
	}
	
	if (abs(UV.y - y2) < 0.01) {
		COLOR += vec4(0, 0.2, 0, 0);
	}
	
	if (abs(UV.y - y3) < 0.01) {
		COLOR += vec4(0, 0.2, 0, 0);
	}
}
