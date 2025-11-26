class TaskProgressMonitor {
    taskProgressHub = '/hubs/task-progress';
    constructor() {
        this.connection = null;
        this.subscribedTasks = new Map();
        this.isConnected = false;

        this.initializeElements();
        this.initializeEventListeners();
    }

    initializeElements() {
        this.connectionStatus = document.getElementById('connectionStatus');
        this.taskIdInput = document.getElementById('taskIdInput');
        this.subscribeBtn = document.getElementById('subscribeBtn');
        this.unsubscribeBtn = document.getElementById('unsubscribeBtn');
        this.connectBtn = document.getElementById('connectBtn');
        this.disconnectBtn = document.getElementById('disconnectBtn');
        this.clearLogsBtn = document.getElementById('clearLogsBtn');
        this.tasksList = document.getElementById('tasksList');
        this.logsContainer = document.getElementById('logsContainer');
    }

    initializeEventListeners() {
        this.connectBtn.addEventListener('click', () => this.connect());
        this.disconnectBtn.addEventListener('click', () => this.disconnect());
        this.subscribeBtn.addEventListener('click', () => this.subscribeToTask());
        this.unsubscribeBtn.addEventListener('click', () => this.unsubscribeFromTask());
        this.clearLogsBtn.addEventListener('click', () => this.clearLogs());

        this.taskIdInput.addEventListener('input', () => this.updateButtonStates());
        this.taskIdInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.subscribeToTask();
            }
        });
    }

    async connect() {
        try {
            this.log('Connecting to SignalR hub...', 'info');
            this.updateConnectionStatus('connecting', 'Connecting...');

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.taskProgressHub)
                .withAutomaticReconnect()
                .build();

            this.connection.on('UpdateProgress', (taskId, progress) => {
                this.handleProgressUpdate(taskId, progress);
            });

            this.connection.onreconnecting(() => {
                this.log('Connection lost. Reconnecting...', 'warning');
                this.updateConnectionStatus('connecting', 'Reconnecting...');
            });

            this.connection.onreconnected(() => {
                this.log('Connection reestablished', 'info');
                this.updateConnectionStatus('connected', 'Connected');
                this.resubscribeToTasks();
            });

            this.connection.onclose(() => {
                this.log('Connection closed', 'warning');
                this.updateConnectionStatus('disconnected', 'Disconnected');
                this.isConnected = false;
                this.updateButtonStates();
            });

            await this.connection.start();

            this.log('Successfully connected to SignalR hub', 'info');
            this.updateConnectionStatus('connected', 'Connected');
            this.isConnected = true;
            this.updateButtonStates();

        } catch (error) {
            this.log(`Connection failed: ${error.message}`, 'error');
            this.updateConnectionStatus('disconnected', 'Connection Failed');
            this.isConnected = false;
            this.updateButtonStates();
        }
    }

    async disconnect() {
        if (this.connection) {
            try {
                for (const taskId of this.subscribedTasks.keys()) {
                    await this.unsubscribeFromTask(taskId, false);
                }

                await this.connection.stop();
                this.log('Disconnected from SignalR hub', 'info');
            } catch (error) {
                this.log(`Disconnect error: ${error.message}`, 'error');
            }
        }
        this.updateConnectionStatus('disconnected', 'Disconnected');
        this.isConnected = false;
        this.updateButtonStates();
    }

    async subscribeToTask() {
        const taskId = this.taskIdInput.value.trim();

        if (!taskId) {
            this.log('Please enter a Task ID', 'warning');
            return;
        }

        if (this.subscribedTasks.has(taskId)) {
            this.log(`Already subscribed to task: ${taskId}`, 'warning');
            return;
        }

        try {
            this.subscribedTasks.set(taskId, {
                progress: 0,
                element: this.createTaskCard(taskId)
            });
            
            await this.connection.invoke('SubscribeToTask', taskId);

            this.log(`Subscribed to task: ${taskId}`, 'info');
            this.taskIdInput.value = '';
            this.updateButtonStates();

        } catch (error) {
            this.log(`Subscribe failed for ${taskId}: ${error.message}`, 'error');
        }
    }

    async unsubscribeFromTask(taskId = null, updateInput = true) {
        const targetTaskId = taskId || this.taskIdInput.value.trim();

        if (!targetTaskId) {
            this.log('Please enter a Task ID', 'warning');
            return;
        }

        if (!this.subscribedTasks.has(targetTaskId)) {
            this.log(`Not subscribed to task: ${targetTaskId}`, 'warning');
            return;
        }

        try {
            await this.connection.invoke('UnsubscribeFromTask', targetTaskId);

            // Remove task from tracking
            const taskElement = this.subscribedTasks.get(targetTaskId).element;
            taskElement.remove();
            this.subscribedTasks.delete(targetTaskId);

            this.log(`Unsubscribed from task: ${targetTaskId}`, 'info');

            if (updateInput) {
                this.taskIdInput.value = targetTaskId;
            }
            this.updateButtonStates();

        } catch (error) {
            this.log(`Unsubscribe failed for ${targetTaskId}: ${error.message}`, 'error');
        }
    }

    async resubscribeToTasks() {
        if (this.subscribedTasks.size > 0) {
            this.log('Resubscribing to tasks...', 'info');

            for (const taskId of this.subscribedTasks.keys()) {
                try {
                    await this.connection.invoke('SubscribeToTask', taskId);
                    this.log(`Resubscribed to task: ${taskId}`, 'info');
                } catch (error) {
                    this.log(`Resubscribe failed for ${taskId}: ${error.message}`, 'error');
                }
            }
        }
    }

    handleProgressUpdate(taskId, progress) {
        if (this.subscribedTasks.has(taskId)) {
            const task = this.subscribedTasks.get(taskId);
            task.progress = progress;

            const progressFill = task.element.querySelector('.progress-fill');
            const progressText = task.element.querySelector('.progress-text');

            if (progressFill) {
                progressFill.style.width = `${progress}%`;
            }
            if (progressText) {
                progressText.textContent = `${progress}%`;
            }

            this.log(`Progress update - ${taskId}: ${progress}%`, 'info');
        }
    }

    createTaskCard(taskId, progress = 0) {
        const taskCard = document.createElement('div');
        taskCard.className = 'task-card';
        taskCard.innerHTML = `
            <div class="task-header">
                <div class="task-id">${taskId}</div>
                <div class="task-status status-subscribed">Subscribed</div>
            </div>
            <div class="progress-container">
                <div class="progress-info">
                    <span>Progress:</span>
                    <span class="progress-text">${progress}%</span>
                </div>
                <div class="progress-bar">
                    <div class="progress-fill" style="width: ${progress}%"></div>
                </div>
            </div>
            <div class="task-actions">
                <button class="btn-danger unsubscribe-single-btn" data-taskid="${taskId}">
                    Unsubscribe
                </button>
            </div>
        `;

        const unsubscribeBtn = taskCard.querySelector('.unsubscribe-single-btn');
        unsubscribeBtn.addEventListener('click', () => {
            this.unsubscribeFromTask(taskId);
        });

        this.tasksList.appendChild(taskCard);
        return taskCard;
    }

    updateConnectionStatus(status, text) {
        this.connectionStatus.textContent = text;
        this.connectionStatus.className = `connection-status ${status}`;
    }

    updateButtonStates() {
        const hasTaskId = this.taskIdInput.value.trim().length > 0;

        this.connectBtn.disabled = this.isConnected;
        this.disconnectBtn.disabled = !this.isConnected;
        this.subscribeBtn.disabled = !this.isConnected || !hasTaskId;
        this.unsubscribeBtn.disabled = !this.isConnected || !hasTaskId;
    }

    log(message, type = 'info') {
        const timestamp = new Date().toLocaleTimeString();
        const logEntry = document.createElement('div');
        logEntry.className = `log-entry log-${type}`;
        logEntry.innerHTML = `<span class="log-time">[${timestamp}]</span> ${message}`;

        this.logsContainer.appendChild(logEntry);
        this.logsContainer.scrollTop = this.logsContainer.scrollHeight;
    }

    clearLogs() {
        this.logsContainer.innerHTML = '';
        this.log('Logs cleared', 'info');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new TaskProgressMonitor();
});