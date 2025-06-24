import React, { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  CircularProgress,
} from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTasks, createTask, updateTask, deleteTask } from '../services/api';
import { Task } from '../types';

const Tasks: React.FC = () => {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Partial<Task> | null>(null);

  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks'],
    queryFn: getTasks,
  });

  const createMutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      handleClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, task }: { id: number; task: Partial<Task> }) =>
      updateTask(id, task),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      handleClose();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTask,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });

  const handleOpen = (task?: Task) => {
    setEditingTask(task || { taskName: '', rotationRule: '' });
    setOpen(true);
  };

  const handleClose = () => {
    setEditingTask(null);
    setOpen(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask) return;

    if ('id' in editingTask && editingTask.id) {
      updateMutation.mutate({
        id: editingTask.id,
        task: { taskName: editingTask.taskName, rotationRule: editingTask.rotationRule },
      });
    } else {
      createMutation.mutate({
        taskName: editingTask.taskName!,
        rotationRule: editingTask.rotationRule!,
      });
    }
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <h1>Tasks</h1>
        <Button variant="contained" color="primary" onClick={() => handleOpen()}>
          Add Task
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Task Name</TableCell>
              <TableCell>Rotation Rule</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {tasks?.map((task) => (
              <TableRow key={task.id}>
                <TableCell>{task.taskName}</TableCell>
                <TableCell>{task.rotationRule}</TableCell>
                <TableCell align="right">
                  <IconButton onClick={() => handleOpen(task)}>
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    onClick={() => {
                      if (window.confirm('Are you sure you want to delete this task?')) {
                        deleteMutation.mutate(task.id);
                      }
                    }}
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>{editingTask?.id ? 'Edit Task' : 'Add Task'}</DialogTitle>
        <form onSubmit={handleSubmit}>
          <DialogContent>
            <TextField
              autoFocus
              margin="dense"
              label="Task Name"
              fullWidth
              value={editingTask?.taskName || ''}
              onChange={(e) =>
                setEditingTask((prev) => ({ ...prev!, taskName: e.target.value }))
              }
            />
            <TextField
              margin="dense"
              label="Rotation Rule"
              fullWidth
              value={editingTask?.rotationRule || ''}
              onChange={(e) =>
                setEditingTask((prev) => ({ ...prev!, rotationRule: e.target.value }))
              }
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingTask?.id ? 'Update' : 'Add'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};

export default Tasks; 