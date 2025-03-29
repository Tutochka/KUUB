import discord
from discord.ext import commands
from discord import app_commands
import datetime

# Constants
GUILD_ID = 1347249457514811453  # Your Guild ID
CATEGORY_ID = 1352606343391084575  # Your Category ID
TRIGGER_VOICE_CHANNEL_ID = 1352594501163679834  # The trigger voice channel ID
WHITELISTED_CHANNELS = {1352594501163679834, 1354901641807007937}  # Whitelisted channel IDs


# Bot initialization
class Client(commands.Bot):
    def __init__(self):
        intents = discord.Intents.default()
        intents.message_content = True
        intents.voice_states = True
        super().__init__(command_prefix="K#", intents=intents)
        self.user_channels = {}

    async def on_ready(self):
        print(f'Logged in as {self.user}!')
        channel = self.get_channel(1354538246964772925)
        if channel:
            await channel.send("**ONLINE**")

        print("STATUS LOGGED")

        # Syncing commands to a specific guild
        try:
            guild = discord.Object(id=GUILD_ID)
            synced = await self.tree.sync(guild=guild)
            print(f'Synced {len(synced)} commands to guild {guild.id}')
            timestamp = int(datetime.datetime.now(datetime.timezone.utc).timestamp())
            print(f"Timestamp: {timestamp}")

            if channel:
                await channel.send(f"SYNCED {len(synced)} COMMANDS AT **<t:{timestamp}:F>**")
        except Exception as e:
            print(f'Error syncing commands: {e}')
            if channel:
                await channel.send(f'Error syncing commands: {e}')

        # Check for and delete empty channels on startup
        guild = self.get_guild(GUILD_ID)
        category = guild.get_channel(CATEGORY_ID)
        if category and isinstance(category, discord.CategoryChannel):
            for channel in category.channels:
                if len(channel.members) == 0 and channel.id not in WHITELISTED_CHANNELS:
                    await channel.delete()
                    print(f"Deleted empty channel: {channel.name}")

    async def on_message(self, message):
        # Check if the message is from a DM and not from the bot itself
        if message.guild is None and message.author != self.user:
            channel = self.get_channel(1354909472493010964)
            if channel:
                await channel.send(f"**DM from {message.author}:** {message.content}")
        await self.process_commands(message)

client = Client()

# Ping command for testing latency
@client.tree.command(name="ping", description="Speed")
@app_commands.guilds(discord.Object(id=GUILD_ID))
async def ping(interaction: discord.Interaction):
    latency = round(client.latency * 1000)
    await interaction.response.send_message(f"Pong! **{latency}**ms")

# Echo command
@client.tree.command(name="echo", description="Echo a message")
@app_commands.guilds(discord.Object(id=GUILD_ID))
async def echo(interaction: discord.Interaction, message: str):
    await interaction.response.send_message(message)

# Handling voice state updates
@client.event
async def on_voice_state_update(member, before, after):
    guild = member.guild
    category = guild.get_channel(CATEGORY_ID)

    # If the user joined the trigger voice channel
    if after.channel and after.channel.id == TRIGGER_VOICE_CHANNEL_ID:
        if category and isinstance(category, discord.CategoryChannel):
            # Create a new voice channel under the correct category
            new_channel = await guild.create_voice_channel(
                name=f"{member.display_name} voice channel",
                category=category
            )
            # Move the user to the newly created channel
            await member.move_to(new_channel)
            print(f"Created channel {new_channel.name} and moved {member.name}.")

            # Store the creator of the channel
            client.user_channels[new_channel.id] = member.id

    # If the user left a voice channel
    if before.channel:
        channel = before.channel
        if channel.category_id == CATEGORY_ID and channel.id not in WHITELISTED_CHANNELS:
            if len(channel.members) == 0:  # If empty, delete it
                await channel.delete()
                print(f"Deleted empty channel: {channel.name}")






