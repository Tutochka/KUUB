import discord
from discord.ext import commands
from discord import app_commands
import datetime
import json
import os
import sys
from dotenv import load_dotenv


# Constants
GUILD_ID = 1347249457514811453
CATEGORY_ID = 1352606343391084575
TRIGGER_VOICE_CHANNEL_ID = 1352594501163679834
WHITELISTED_CHANNELS = {1352594501163679834, 1354901641807007937}
LOG_FILE = "messages.json"
CHIEF_ENGINEER_ROLE = 1356710543624241252


class Client(commands.Bot):
    def __init__(self):
        intents = discord.Intents.default()
        intents.message_content = True
        intents.voice_states = True
        super().__init__(command_prefix="K#", intents=intents)
        self.user_channels = {}

    async def on_ready(self):
        """Called when the bot is ready."""
        print(f'Logged in as {self.user}!')
        await self.send_status_message()
        await self.sync_commands()
        await self.check_and_delete_empty_channels()

    async def send_status_message(self):
        """Send status update to a channel when bot is online."""
        channel = self.get_channel(1354538246964772925)
        if channel:
            await channel.send("**ONLINE**")
        print("STATUS LOGGED")

    async def sync_commands(self):
        """Sync bot commands to a specific guild."""
        try:
            guild = discord.Object(id=GUILD_ID)
            synced = await self.tree.sync(guild=guild)
            print(f'Synced {len(synced)} commands to guild {guild.id}')
            timestamp = int(datetime.datetime.now(datetime.timezone.utc).timestamp())
            await self.send_sync_message(synced, timestamp)
        except Exception as e:
            print(f'Error syncing commands: {e}')
            await self.send_sync_error_message(e)

    async def send_sync_message(self, synced, timestamp):
        """Send the sync status to a channel."""
        channel = self.get_channel(1354538246964772925)
        if channel:
            await channel.send(f"SYNCED {len(synced)} COMMANDS AT **<t:{timestamp}:F>**")

    async def send_sync_error_message(self, error):
        """Send error message if sync fails."""
        channel = self.get_channel(1354538246964772925)
        if channel:
            await channel.send(f'Error syncing commands: {error}')

    async def check_and_delete_empty_channels(self):
        """Check for empty channels and delete them."""
        guild = self.get_guild(GUILD_ID)
        category = guild.get_channel(CATEGORY_ID)
        if category and isinstance(category, discord.CategoryChannel):
            for channel in category.channels:
                if len(channel.members) == 0 and channel.id not in WHITELISTED_CHANNELS:
                    await channel.delete()
                    print(f"Deleted empty channel: {channel.name}")

    async def on_message(self, message):
        """Handle incoming messages, log them, and process commands."""
        if message.author.bot:
            return
        await self.process_dm_messages(message)
        await self.process_commands(message)
        await self.handle_image_attachments(message)

    async def process_dm_messages(self, message):
        """Handle messages from DMs."""
        if message.guild is None and message.author != self.user:
            channel = self.get_channel(1354909472493010964)
            if channel:
                await channel.send(f"**DM from {message.author}:** {message.content}")


    async def handle_image_attachments(self, message):
        """Handle image attachments and send info to a specific channel."""
        for attachment in message.attachments:
            if attachment.url:
                await self.send_image_info(message, attachment)

    async def send_image_info(self, message, attachment):
        """Send image details to a specific channel."""
        file_name = attachment.filename
        url = attachment.url
        size = attachment.size
        width = attachment.width if hasattr(attachment, 'width') else 'N/A'
        height = attachment.height if hasattr(attachment, 'height') else 'N/A'
        author = message.author.name

        # Channel location
        if message.guild:
            channel_location = f"Server: {message.guild.name} - Channel: {message.channel.name}"
        else:
            channel_location = f"DM with {message.author.name}"

        image_info = (
            f"**Image Info**\n"
            f"**Author**: {author}\n"
            f"**File Name**: {file_name}\n"
            f"**Size**: {size} bytes\n"
            f"**URL**: [Click to view]({url})\n"
            f"**Dimensions**: {width}x{height} pixels\n"
            f"**Channel Location**: {channel_location}"
        )

        print(image_info)
        channel = self.get_channel(1354909472493010964)  # Replace with your channel ID
        if channel:
            await channel.send(image_info)

    async def on_voice_state_update(self, member, before, after):
        """Handle voice state updates for dynamic voice channels."""
        guild = member.guild
        category = guild.get_channel(CATEGORY_ID)

        if after.channel and after.channel.id == TRIGGER_VOICE_CHANNEL_ID:
            await self.create_and_move_to_new_channel(member, category)

        if before.channel and before.channel.id != TRIGGER_VOICE_CHANNEL_ID:
            await self.delete_empty_channels(before)

    async def create_and_move_to_new_channel(self, member, category):
        """Create a new voice channel and move the user to it."""
        if category and isinstance(category, discord.CategoryChannel):
            new_channel = await member.guild.create_voice_channel(
                name=f"{member.display_name} voice channel",
                category=category
            )
            await member.move_to(new_channel)
            print(f"Created channel {new_channel.name} and moved {member.name}.")
            self.user_channels[new_channel.id] = member.id

    async def delete_empty_channels(self, before):
        """Delete empty channels."""
        channel = before.channel
        if channel.category_id == CATEGORY_ID and channel.id not in WHITELISTED_CHANNELS:
            if len(channel.members) == 0:
                await channel.delete()
                print(f"Deleted empty channel: {channel.name}")
    async def log(self, message, channel=1354538246964772925):
        embed = discord.Embed(title="Log", description=message)
        try:
            channel_obj = await self.fetch_channel(channel)
            if channel_obj:
                await channel_obj.send(embed=embed)
            else:
                print("Channel not found.")
        except Exception as e:
            print(f"Error fetching channel: {e}")



client = Client()

# Commands
@client.tree.command(name="ping", description="Speed")
@app_commands.guilds(discord.Object(id=GUILD_ID))
async def ping(interaction: discord.Interaction):
    latency = round(client.latency * 1000)
    await interaction.response.send_message(f"Pong! **{latency}**ms")

@client.tree.command(name="echo", description="Echo a message")
@app_commands.guilds(discord.Object(id=GUILD_ID))
async def echo(interaction: discord.Interaction, message: str):
    await interaction.response.send_message(message)

@client.tree.command(name="reboot", description="Live Reboot")
@app_commands.guilds(discord.Object(id=GUILD_ID))
async def reboot(interaction: discord.Interaction):
    if not any(role.id == CHIEF_ENGINEER_ROLE for role in interaction.user.roles):
        await interaction.response.send_message("❌ You don't have permission to do this.", ephemeral=True)
        return
    await interaction.response.send_message("Rebooting...", ephemeral=True)
    await client.log(f"Rebooting command executed by **{interaction.user}**")
    os.execv(sys.executable, ['python'] + sys.argv)


load_dotenv()
TOKEN = os.getenv("DISCORD_TOKEN")
client.run(TOKEN)
