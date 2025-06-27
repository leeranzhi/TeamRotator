import React, { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Stack,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, AssignmentInd as AssignIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getTasks, createTask, updateTask, getMembers, assignTask, updateAssignment } from '../services/api';
import { Task, Member, ModifyAssignment } from '../types';

const Tasks: React.FC = () => {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Partial<Task> | null>(null);
  const [selectedTaskId, setSelectedTaskId] = useState<number | null>(null);
  const [selectedMemberId, setSelectedMemberId] = useState<number>(-1);

  const { data: tasks } = useQuery<Task[]>({
    queryKey: ['tasks'],
    queryFn: getTasks,
  });

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: getMembers,
  });

  const createMutation = useMutation({
    mutationFn: (task: Omit<Task, 'id'>) => createTask(task),
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

  const assignMutation = useMutation({
    mutationFn: ({ taskId, memberId }: { taskId: number; memberId: number }) =>
      assignTask(taskId, memberId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
      handleAssignDialogClose();
    },
  });

  const updateAssignmentMutation = useMutation({
    mutationFn: ({ id, assignment }: { id: number; assignment: ModifyAssignment }) =>
      updateAssignment(id, assignment),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
      handleAssignDialogClose();
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

  const handleAssignDialogOpen = (taskId: number) => {
    setSelectedTaskId(taskId);
    setSelectedMemberId(-1);
    setAssignDialogOpen(true);
  };

  const handleAssignDialogClose = () => {
    setSelectedTaskId(null);
    setSelectedMemberId(-1);
    setAssignDialogOpen(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask) return;

    if (editingTask.id) {
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

  const handleAssign = () => {
    if (selectedTaskId && selectedMemberId >= 0) {
      const selectedMember = members?.find(m => m.id === selectedMemberId);
      if (selectedMember) {
        updateAssignmentMutation.mutate({
          id: selectedTaskId,
          assignment: { host: selectedMember.host }
        });
      }
    }
  };

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Tasks</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => handleOpen()}
        >
          Add Task
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
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
                  <Stack direction="row" spacing={1} justifyContent="flex-end">
                    <IconButton onClick={() => handleOpen(task)}>
                      <EditIcon />
                    </IconButton>
                    <IconButton onClick={() => handleAssignDialogOpen(task.id)}>
                      <AssignIcon />
                    </IconButton>
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose}>
        <form onSubmit={handleSubmit}>
          <DialogTitle>{editingTask?.id ? 'Edit Task' : 'Add Task'}</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2 }}>
              <TextField
                autoFocus
                margin="dense"
                label="Name"
                fullWidth
                value={editingTask?.taskName || ''}
                onChange={(e) =>
                  setEditingTask((prev) => ({ ...prev!, taskName: e.target.value }))
                }
              />
              <FormControl fullWidth margin="dense">
                <InputLabel>Rotation Rule</InputLabel>
                <Select
                  value={editingTask?.rotationRule || ''}
                  onChange={(e) =>
                    setEditingTask((prev) => ({ ...prev!, rotationRule: e.target.value }))
                  }
                  label="Rotation Rule"
                >
                  <MenuItem value="daily">Daily</MenuItem>
                  <MenuItem value="weekly_monday">Weekly (Monday)</MenuItem>
                  <MenuItem value="biweekly_monday">Biweekly (Monday)</MenuItem>
                </Select>
              </FormControl>
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingTask?.id ? 'Save' : 'Add'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      <Dialog open={assignDialogOpen} onClose={handleAssignDialogClose}>
        <DialogTitle>Assign Task</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <FormControl fullWidth margin="dense">
              <InputLabel>Member</InputLabel>
              <Select
                value={selectedMemberId}
                onChange={(e) => setSelectedMemberId(Number(e.target.value))}
                label="Member"
              >
                <MenuItem value={-1} disabled>Select a member</MenuItem>
                {members?.map((member) => (
                  <MenuItem key={member.id} value={member.id}>
                    {member.host}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleAssignDialogClose}>Cancel</Button>
          <Button
            onClick={handleAssign}
            variant="contained"
            color="primary"
            disabled={selectedMemberId < 0}
          >
            Assign
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Tasks; 