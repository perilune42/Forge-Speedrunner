using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class DoorwayEditor : MonoBehaviour
{
    // I don't know how to access RoomManager to get these fields, if it's even
    // possible in the first place, so I'm just going to hard-code them here for now.
    int BaseWidth = 64, BaseHeight = 36;

    (Vector3, Quaternion)? oldTransform = null;

    void Update()
    {
        if (transform.hasChanged) {
            // TODO: We can't use the Doorway.enclosingRoom to figure out the room,
            // since that's not set in the editor. We should probably control the
            // enclosingRoom state from here, instead of the other way around.
            Room room = GetComponentInParent<Room>();

            if (room == null) {
                transform.hasChanged = false;
                return;
            }

            Vector2 roomSize = (Vector2) room.size;

            Vector3 pos = transform.position;
            Quaternion rot = Quaternion.identity;

            pos = room.transform.InverseTransformPoint(pos);

            float tgtX = pos.x / BaseWidth;
            float tgtY = pos.y / BaseHeight;

            // Compute cross products with the diagonals of the room rectangle.
            bool topOrLeft  = (roomSize.x * tgtY - roomSize.y * tgtX) > 0;
            bool topOrRight = (roomSize.x * tgtY - roomSize.y * (roomSize.x - tgtX)) > 0;

            Edge edge = topOrLeft ? (topOrRight ? Edge.UP : Edge.LEFT) : (topOrRight ? Edge.RIGHT : Edge.DOWN);

            List<Doorway> doorList;
            int doorIdx;
            string doorName;

            // Find the door index and snapping coordinates.
            if (edge == Edge.UP || edge == Edge.DOWN) {
                doorIdx = (int) Mathf.Round(pos.x / BaseWidth - 0.5f);
                doorIdx = Mathf.Clamp(doorIdx, 0, room.size.x - 1);

                pos.x = (doorIdx + 0.5f) * BaseWidth;

                if (edge == Edge.UP) {
                    doorName = "Up";
                    doorList = room.doorwaysUp;
                    pos.y = roomSize.y * BaseHeight;
                } else {
                    doorName = "Down";
                    doorList = room.doorwaysDown;
                    pos.y = 0f;
                }
            } else {
                rot = Quaternion.Euler(0, 0, 90);

                doorIdx = (int) Mathf.Round(pos.y / BaseHeight - 0.5f);
                doorIdx = Mathf.Clamp(doorIdx, 0, room.size.y - 1);

                pos.y = (doorIdx + 0.5f) * BaseHeight;

                if (edge == Edge.RIGHT) {
                    doorName = "Right";
                    doorList = room.doorwaysRight;
                    pos.x = roomSize.x * BaseWidth;
                } else {
                    doorName = "Left";
                    doorList = room.doorwaysLeft;
                    pos.x = 0f;
                }
            }

            pos = room.transform.TransformPoint(pos);

            // Pad the door list with nulls for empty door slots.
            while (doorList.Count <= doorIdx)
                doorList.Add(null);

            Doorway doorway = GetComponent<Doorway>();

            if (doorList[doorIdx] != null && doorList[doorIdx] != doorway) {
                transform.hasChanged = false;

                if (oldTransform.HasValue) {
                    transform.position = oldTransform.Value.Item1;
                    transform.rotation = oldTransform.Value.Item2;
                } else {
                    // Assume a door without previous location datais being dragged
                    // from the prefab menu, and so just delete it if it conflicts.
                    DestroyImmediate(gameObject);
                }

                return;
            }

            // TODO: if a door is moved from a different room to this one, it will
            // remain in that room's doorways list, which will cause issues. The index
            // and list should be tracked consistently instead of looking it up here.
            // This can be accomplished by tracking the room we used to be contained in.
            List<Doorway> oldList = null;
            int oldIdx;

            if      ((oldIdx =    room.doorwaysUp.IndexOf(doorway)) != -1) oldList = room.doorwaysUp;
            else if ((oldIdx =  room.doorwaysDown.IndexOf(doorway)) != -1) oldList = room.doorwaysDown;
            else if ((oldIdx =  room.doorwaysLeft.IndexOf(doorway)) != -1) oldList = room.doorwaysLeft;
            else if ((oldIdx = room.doorwaysRight.IndexOf(doorway)) != -1) oldList = room.doorwaysRight;

            // Update the name of the door.
            gameObject.name = doorName[0].ToString() + doorIdx;

            // Move the door to the correct list.
            if (oldList != null) oldList[oldIdx] = null;
            doorList[doorIdx] = doorway;

            transform.position = pos;
            transform.rotation = rot;
        
            oldTransform = (pos, rot);

            transform.hasChanged = false;

            // TODO: Set the correct parent for the doorway. This is kind of complicated
            // do to given that we'll be working inside prefabs, and will need to load,
            // edit, and save the prefab programmatically.
        }
    }
}
