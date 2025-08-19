@tool
extends EditorPlugin

const Convertor = preload("res://Game/Code/ThirdParty/AS2P/InspectorConvertor.gd")

var plugin: Convertor

func _enter_tree_editor():
	plugin = Convertor.new()
	plugin.animation_updated.connect(_refresh, CONNECT_DEFERRED)
	add_inspector_plugin(plugin)

func _refresh(anim_player):
	var interface = get_editor_interface()

	# Hacky way to force the editor to deselect and reselect
	#	the animation panel, as the panel won't update until then
	interface.inspect_object(interface.get_edited_scene_root())
	interface.get_selection().clear()
	await get_tree().create_timer(0.05).timeout
	interface.inspect_object(anim_player)

func _exit_tree_editor():
	remove_inspector_plugin(plugin)

