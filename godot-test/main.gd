extends Node

func _ready():
	print("WW3_TEST: Escape scene ready - Godot driving via MCP")
	print("WW3_TEST: Engine=", Engine.get_version_info().string)
	get_tree().create_timer(20.0).timeout.connect(_on_test_done)

func _on_test_done():
	print("WW3_TEST: scene ran 3s, quitting cleanly")
	get_tree().quit()
