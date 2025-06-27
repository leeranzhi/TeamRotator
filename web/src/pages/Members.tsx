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
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '../services/api';
import { Member } from '../types';

const Members: React.FC = () => {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editingMember, setEditingMember] = useState<Partial<Member> | null>(null);

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: () => api.get<Member[]>('/members').then((res) => res.data),
  });

  const createMutation = useMutation({
    mutationFn: (member: Omit<Member, 'id'>) =>
      api.post<Member>('/members', member).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members'] });
      handleClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, member }: { id: number; member: Partial<Member> }) =>
      api.put<Member>(`/members/${id}`, member).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members'] });
      handleClose();
    },
  });

  const handleOpen = (member?: Member) => {
    setEditingMember(member || { name: '', slackId: '' });
    setOpen(true);
  };

  const handleClose = () => {
    setEditingMember(null);
    setOpen(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingMember) return;

    if (editingMember.id) {
      updateMutation.mutate({
        id: editingMember.id,
        member: { name: editingMember.name, slackId: editingMember.slackId },
      });
    } else {
      createMutation.mutate({
        name: editingMember.name!,
        slackId: editingMember.slackId!,
      });
    }
  };

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Team Members</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => handleOpen()}
        >
          Add Member
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Slack ID</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {members?.map((member) => (
              <TableRow key={member.id}>
                <TableCell>{member.name}</TableCell>
                <TableCell>{member.slackId}</TableCell>
                <TableCell align="right">
                  <IconButton onClick={() => handleOpen(member)}>
                    <EditIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose}>
        <form onSubmit={handleSubmit}>
          <DialogTitle>{editingMember?.id ? 'Edit Member' : 'Add Member'}</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2 }}>
              <TextField
                autoFocus
                margin="dense"
                label="Name"
                fullWidth
                value={editingMember?.name || ''}
                onChange={(e) =>
                  setEditingMember((prev) => ({ ...prev!, name: e.target.value }))
                }
              />
              <TextField
                margin="dense"
                label="Slack ID"
                fullWidth
                value={editingMember?.slackId || ''}
                onChange={(e) =>
                  setEditingMember((prev) => ({ ...prev!, slackId: e.target.value }))
                }
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingMember?.id ? 'Save' : 'Add'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};

export default Members; 