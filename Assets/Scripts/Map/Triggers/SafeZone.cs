public class SafeZone : Trigger
{
    private void FixedUpdate()
    {
        if (!playerInside) return;
        if (Player.Instance.Movement.State == BodyState.OnGround)
        {
            RoomManager.Instance.RespawnPosition = Player.Instance.Movement.transform.position;
        }
        
    }
}